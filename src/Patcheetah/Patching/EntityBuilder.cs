using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using Patcheetah.Mapping;

namespace Patcheetah.Patching
{
    public static class EntityBuilder<TEntity> where TEntity : class, new()
    {
        public static void PatchEntity(TEntity model, IDictionary<string, object> patchData, EntityConfig config)
        {
            Build(patchData, typeof(TEntity), config, model);
        }

        public static TEntity BuildEntity(IDictionary<string, object> patchData, EntityConfig config)
        {
            var result = Build(patchData, typeof(TEntity), config);

            return result as TEntity;
        }

        public static TEntity BuildEntity(TEntity model, IDictionary<string, object> patchData, EntityConfig config)
        {
            var result = Build(patchData, typeof(TEntity), config, model);

            return result as TEntity;
        }

        public static object Build(IDictionary<string, object> data, Type type, EntityConfig config, object entityToPatch = null)
        {
            var properties = type.GetProperties();
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

        private static void ProcessProperty(PropertyInfo property, object entityToPatch, IDictionary<string, object> patchData, EntityConfig config, bool entityBuiltFromScratch)
        {
            // If config is null, we gonna use simple patching
            if (config == null)
            {
                if (!patchData.ContainsKey(property.Name))
                {
                    return;
                }

                property.SetValue(entityToPatch, patchData[property.Name]);

                return;
            }

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

            if (config.KeyPropertyName == propertyName)
            {
                if (newValue == null)
                {
                    throw new InvalidKeyException("Key property value can't be null");
                }

                if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
                {
                    throw new InvalidKeyException(property.PropertyType);
                }

                if (!entityBuiltFromScratch)
                {
                    if (config.CheckKeyOnPatching && (oldValue as IComparable).CompareTo(newValue) != 0)
                    {
                        throw new KeyConcurrenceException(newValue);
                    }
                }
            }

            if (newValue == null)
            {
                object nullValue = null;

                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                {
                    nullValue = Activator.CreateInstance(property.PropertyType);
                }

                property.SetValue(entityToPatch, nullValue);

                return;
            }

            if (!entityBuiltFromScratch)
            {
                InvokeBeforePatchCallback(propertyConfig, entityToPatch, oldValue, newValue);
            }

            newValue = MapValue(newValue, property, propertyConfig);

            foreach (var mutator in MutatorsContainer.Instance.Mutators)
            {
                newValue = mutator.MutateNewValueForProperty(newValue, property, propertyConfig, oldValue);
            }

            property.SetValue(entityToPatch, newValue);

            if (!entityBuiltFromScratch)
            {
                InvokeAfterPatchCallback(propertyConfig, entityToPatch, oldValue, newValue);
            }
        }

        private static void InvokeBeforePatchCallback(PropertyConfiguration config, object patchedEntity, object oldPropertyValue, object newPropertyValue)
        {
            config.BeforePatchCallback?.Invoke(new PropertyChangedEventArgs
            {
                Entity = patchedEntity,
                SourceValue = oldPropertyValue,
                TargetValue = newPropertyValue
            });
        }

        private static void InvokeAfterPatchCallback(PropertyConfiguration config, object patchedEntity, object oldPropertyValue, object newPropertyValue)
        {
            config.AfterPatchCallback?.Invoke(new PropertyChangedEventArgs
            {
                Entity = patchedEntity,
                SourceValue = oldPropertyValue,
                TargetValue = newPropertyValue
            });
        }

        private static object MapValue(object value, PropertyInfo propertyInfo, PropertyConfiguration configuration)
        {
            var typeName = propertyInfo.PropertyType.Name;
            var globalHandler = MappingHandlersContainer.Instance.GlobalMappingHandler;
            var typeHandler = MappingHandlersContainer.Instance.GetHandler(propertyInfo.PropertyType);
            var propertyHandler = configuration.MappingHandler;

            if (globalHandler != null)
            {
                var globalHandlerResult = globalHandler(value);
                if (!globalHandlerResult.Skip)
                    value = globalHandlerResult.Value;
            }

            if (typeHandler != null)
            {
                var typeHandlerResult = typeHandler(value);
                if (!typeHandlerResult.Skip)
                    value = typeHandlerResult.Value;
            }

            if (propertyHandler != null)
            {
                var propertyHandlerResult = propertyHandler(value);
                if (!propertyHandlerResult.Skip)
                    value = propertyHandlerResult.Value;
            }

            return value;
        }
    }
}
