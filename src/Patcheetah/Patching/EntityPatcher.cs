﻿using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Patching
{
    public sealed class EntityPatcher<TEntity> 
        where TEntity : class
    {
        private readonly IJsonTypesResolver _jsonTypesResolver;

        public static EntityPatcher<TEntity> Create() => new EntityPatcher<TEntity>(PatchEngineCore.JsonTypesResolver);

        public EntityPatcher(IJsonTypesResolver jsonTypesResolver)
        {
            _jsonTypesResolver = jsonTypesResolver;
        }

        public TEntity BuildNew(IDictionary<string, object> patchData, EntityConfig config)
        {
            var result = BuildEntity(patchData, typeof(TEntity), config);

            return result as TEntity;
        }

        public void Patch(TEntity model, IDictionary<string, object> patchData, EntityConfig config)
        {
            BuildEntity(patchData, typeof(TEntity), config, model);
        }

        public object BuildEntity(IDictionary<string, object> data, Type type, EntityConfig config, object entityToPatch = null)
        {
            var properties = type.GetProperties();
            var newEntityCreated = false;

            if (entityToPatch == null)
            {
                newEntityCreated = true;
                entityToPatch = Activator.CreateInstance(type);
            }

            var configuredProperties = config?.ConfiguredProperties;
            var missedRequiredPropertyConfigs = configuredProperties?
                .Where(x => x.Required && !data.Keys.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            if (missedRequiredPropertyConfigs?.Any() ?? false)
            {
                throw new RequiredPropertiesMissedException(missedRequiredPropertyConfigs.Select(x => x.Name).ToArray());
            }

            if (!newEntityCreated && configuredProperties != null)
            {
                foreach (var ignored in configuredProperties.Where(x => x.Ignored).Select(x => x.Name))
                {
                    data.Remove(ignored);
                }
            }

            foreach (var property in properties)
            {
                ProcessProperty(property, entityToPatch, data, config, newEntityCreated);
            }

            return entityToPatch;
        }

        private void ProcessProperty(PropertyInfo property, object entityToPatch, IDictionary<string, object> patchData, EntityConfig config, bool entityBuiltFromScratch)
        {
            var propertyConfig = config?[property.Name];
            var propertyName = propertyConfig?.Name ?? property.Name;

            if (propertyName.StartsWith("_")) propertyName = propertyName.TrimStart('_');  //this fixes where propertyNames have had an underscore appended (because of numeric values in JSON) but will break any properties which actually start with underscore!

            if (!patchData.ContainsKey(propertyName))
            {
                return;
            }

            var newValue = patchData[propertyName];
            var isObject = _jsonTypesResolver.IsObject(newValue);
            var oldValue = property.GetValue(entityToPatch);
            var prePatchFunc = PatchEngineCore.Config.PrePatchProcessingFunc;

            if (prePatchFunc != null && (!PatchEngineCore.Config.RFC7396Enabled || !isObject))
            {
                newValue = _jsonTypesResolver.ResolveType(newValue, property.PropertyType);
                newValue = prePatchFunc(new PatchContext
                {
                    NewValue = newValue,
                    OldValue = oldValue,
                    Entity = entityToPatch,
                    PropertyConfiguration = propertyConfig
                });
            }

            if (propertyConfig == null)
            {
                if (isObject && PatchEngineCore.Config.RFC7396Enabled)
                {
                    newValue = PatchNested(newValue, oldValue, property);
                }

                TrySet(newValue, entityToPatch, property);

                return;
            }

            // key check
            if (config.KeyPropertyName == propertyName && config.CheckKeyOnPatching)
            {
                if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
                {
                    throw new InvalidKeyException(property.PropertyType);
                }

                if (!entityBuiltFromScratch)
                {
                    if ((oldValue as IComparable).CompareTo(newValue) != 0)
                    {
                        throw new KeyConcurrenceException(newValue);
                    }
                }
            }

            // null value handling
            if (newValue == null)
            {
                object defaultValue = null;

                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                {
                    defaultValue = Activator.CreateInstance(property.PropertyType);
                }

                property.SetValue(entityToPatch, defaultValue);

                return;
            }

            if (isObject && PatchEngineCore.Config.RFC7396Enabled)
            {
                newValue = PatchNested(newValue, oldValue, property);
            }

            newValue = _jsonTypesResolver.ResolveType(newValue, property.PropertyType);
            newValue = MapValue(newValue, property.PropertyType, propertyConfig);
            TrySet(newValue, entityToPatch, property, false);
        }

        private void TrySet(object newValue, object entityToPatch, PropertyInfo property, bool resolveType = true)
        {
            try
            {
                if (resolveType)
                {
                    newValue = _jsonTypesResolver.ResolveType(newValue, property.PropertyType);
                }

                //find the property type
                Type propertyType = property.PropertyType;

                //null value handling
                if (newValue == null)
                {
                    if (IsNullableType(propertyType)) property.SetValue(entityToPatch, null);  //where property is nullable, we can set null
                    else throw new ArgumentNullException($"Cannot set property {property.Name} to null");
                }
                else
                {
                    //Convert.ChangeType does not handle conversion to nullable types
                    //if the property type is nullable, we need to get the underlying type of the property
                    var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

                    //Returns an System.Object with the specified System.Type and whose value is
                    //equivalent to the specified object.
                    newValue = Convert.ChangeType(newValue, targetType);

                    property.SetValue(entityToPatch, newValue);
                }

            }
            catch (ArgumentException ex)
            {
                throw new TypeMissmatchException(newValue.GetType().Name, property.PropertyType.Name, property.Name, ex);
            }
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        private object PatchNested(object newValue, object oldValue, PropertyInfo property)
        {
            var nestedConfig = PatchEngineCore.Config.GetEntityConfig(property.PropertyType);
            var nestedPatchData = _jsonTypesResolver.ResolveJsonType<Dictionary<string, object>>(newValue);

            var caseInsensitiveNestedPatchData = new Dictionary<string, object>(nestedPatchData, StringComparer.OrdinalIgnoreCase);
            newValue = BuildEntity(caseInsensitiveNestedPatchData, property.PropertyType, nestedConfig, oldValue);

            return newValue;
        }

        private static object MapValue(object value, Type valueType, PropertyConfiguration configuration)
        {
            var globalHandler = PatchEngineCore.Config.GlobalMappingHandler;
            var typeHandler = PatchEngineCore.Config.GetMappingHandlerForType(valueType);
            var propertyHandler = configuration.MappingHandler;

            if (globalHandler != null)
            {
                var globalHandlerResult = globalHandler(value);
                if (!globalHandlerResult.Skipped)
                    value = globalHandlerResult.Value;
            }

            if (typeHandler != null)
            {
                var typeHandlerResult = typeHandler(value);
                if (!typeHandlerResult.Skipped)
                    value = typeHandlerResult.Value;
            }

            if (propertyHandler != null)
            {
                var propertyHandlerResult = propertyHandler(value);
                if (!propertyHandlerResult.Skipped)
                    value = propertyHandlerResult.Value;
            }

            return value;
        }

    }
}
