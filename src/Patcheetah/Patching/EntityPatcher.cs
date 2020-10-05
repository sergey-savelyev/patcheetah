using Newtonsoft.Json.Linq;
using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Patching
{
    public sealed class EntityPatcher
    {
        public TEntity BuildNew<TEntity>(IDictionary<string, object> patchData, EntityConfig config)
            where TEntity: class
        {
            var result = BuildEntity(patchData, typeof(TEntity), config);

            return result as TEntity;
        }

        public void Patch<TEntity>(TEntity model, IDictionary<string, object> patchData, EntityConfig config)
            where TEntity : class
        {
            BuildEntity(patchData, typeof(TEntity), config, model);
        }

        public static object BuildEntity(IDictionary<string, object> data, Type type, EntityConfig config, object entityToPatch = null)
        {
            var properties = type.GetProperties();

            // If config is null, we gonna use simple patching
            if (config == null)
            {
                foreach (var property in properties)
                {
                    ProcessProperty(property, entityToPatch, data);
                }

                return entityToPatch;
            }

            var configuredProperties = config.ConfiguredProperties;
            var missedRequiredPropertyNames = configuredProperties
                .Where(x => x.Required)
                .Select(x => x.Name)
                .Except(data.Keys)
                .ToArray();

            if (missedRequiredPropertyNames.Any())
            {
                throw new RequiredPropertiesMissedException(missedRequiredPropertyNames);
            }

            var newEntityCreated = false;

            if (entityToPatch == null)
            {
                newEntityCreated = true;
                entityToPatch = Activator.CreateInstance(type);
            }

            if (!newEntityCreated)
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

        private static void ProcessProperty(PropertyInfo property, object entityToPatch, IDictionary<string, object> patchData)
        {
            if (!patchData.ContainsKey(property.Name))
            {
                return;
            }

            var oldValue = property.GetValue(entityToPatch);
            var newValue = patchData[property.Name];

            newValue = PatchNested(newValue, oldValue, property);
            newValue = ResolveType(newValue, property);

            property.SetValue(entityToPatch, newValue);
        }

        private static void ProcessProperty(PropertyInfo property, object entityToPatch, IDictionary<string, object> patchData, EntityConfig config, bool entityBuiltFromScratch)
        {
            var propertyConfig = config[property.Name];
            var propertyName = propertyConfig?.Name ?? property.Name;

            if (!patchData.ContainsKey(propertyName))
            {
                return;
            }

            var newValue = patchData[propertyName];
            var oldValue = property.GetValue(entityToPatch);

            if (propertyConfig == null)
            {
                property.SetValue(entityToPatch, newValue);
                return;
            }

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

            if (!entityBuiltFromScratch && propertyConfig.BeforeMappingCallback != null)
            {
                newValue = propertyConfig.BeforeMappingCallback.Invoke(new PropertyChangedEventArgs
                {
                    Entity = entityToPatch,
                    OldValue = oldValue,
                    NewValue = newValue,
                    PropertyConfiguration = propertyConfig
                });
            }

            newValue = PatchNested(newValue, oldValue, property);
            newValue = ResolveType(newValue, property);
            newValue = MapValue(newValue, property.PropertyType, propertyConfig);

            property.SetValue(entityToPatch, newValue);

            if (!entityBuiltFromScratch)
            {
                propertyConfig.AfterSetCallback?.Invoke(new PropertyChangedEventArgs
                {
                    Entity = entityToPatch,
                    OldValue = oldValue,
                    NewValue = newValue,
                    PropertyConfiguration = propertyConfig
                });
            }
        }

        private static object PatchNested(object newValue, object oldValue, PropertyInfo property)
        {
            if (newValue is JObject newValueJobject && PatchEngine.Config.RFC7396Enabled)
            {
                var nestedConfig = PatchEngine.Config.GetEntityConfig(property.PropertyType);
                var nestedPatchData = newValueJobject.ToObject<Dictionary<string, object>>();
                var caseInsensitiveNestedPatchData = new Dictionary<string, object>(nestedPatchData, StringComparer.OrdinalIgnoreCase);
                newValue = BuildEntity(caseInsensitiveNestedPatchData, property.PropertyType, nestedConfig, oldValue);
            }

            return newValue;
        }

        private static object ResolveType(object value, PropertyInfo property)
        {
            if (value is JObject newValueJobject)
            {
                value = newValueJobject.ToObject(property.PropertyType);
            }

            if (value is JArray newValueJArray)
            {
                value = newValueJArray.ToObject(property.PropertyType);
            }

            return value;
        }

        private static object MapValue(object value, Type valueType, PropertyConfiguration configuration)
        {
            var globalHandler = PatchEngine.Config.GlobalMappingHandler;
            var typeHandler = PatchEngine.Config.GetMappingHandlerForType(valueType);
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
