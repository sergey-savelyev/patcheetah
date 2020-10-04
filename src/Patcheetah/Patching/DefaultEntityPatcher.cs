using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using Patcheetah.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Patcheetah.Patching
{
    public sealed class DefaultEntityPatcher : IEntityPatcher
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

            if (config.KeyPropertyName == propertyName && config.CheckKeyOnPatching)
            {
                //if (newValue == null)
                //{
                //    throw new InvalidKeyException("Key property value can't be null");
                //}

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
