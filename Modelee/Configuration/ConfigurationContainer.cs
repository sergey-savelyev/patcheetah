using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modelee.Attributes;
using Modelee.Exceptions;
using Newtonsoft.Json;

namespace Modelee.Configuration
{
    internal class ConfigurationContainer
    {
        private static ConfigurationContainer _container;

        public static ConfigurationContainer Instance => _container ?? (_container = new ConfigurationContainer());

        private IDictionary<string, EntityConfig> _configs;

        private ConfigurationContainer()
        {
            _configs = new Dictionary<string, EntityConfig>();
        }

        public void RegisterConfig<TEntity>(EntityConfig config)
        {
            this.RegisterConfigForType(config, typeof(TEntity));
        }

        public EntityConfig GetConfig<TEntity>()
        {
            var type = typeof(TEntity);

            return GetConfig(type);
        }

        public EntityConfig GetConfig(Type type)
        {
            var name = type.Name;

            if (!_configs.ContainsKey(name))
            {
                RegisterConfigForType(null, type);

                return GetConfig(type);
            }

            return _configs[name];
        }

        private void RegisterConfigForType(EntityConfig config, Type type)
        {
            var name = type.Name;
            config = ObtainConfigFromType(type, config);
            ValidateConfig(config);
            _configs.RewriteIfExist(name, config);
        }

        private EntityConfig ObtainConfigFromType(Type type, EntityConfig entityConfig = null)
        {
            var caseSensitive = entityConfig?.CaseSensitive ?? false;
            var caseSensitivityAttrs = type.GetCustomAttributes(typeof(CaseSensitivePatchingAttribute), false);

            if (caseSensitivityAttrs.Any())
            {
                var caseSensitiveAttr = caseSensitivityAttrs.Last();
                caseSensitive = (caseSensitiveAttr as CaseSensitivePatchingAttribute).CaseSensitive;
            }

            if (entityConfig == null)
            {
                entityConfig = new EntityConfig(caseSensitive, type);
            }

            // it has parameter equality comparison inside, so if config was just created, it won't be setted again
            entityConfig.SetCaseSensitive(caseSensitive);

            var properties = type.GetProperties();
            var keyPropertyAttributeFound = false;

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();

                // Alias or jsonProp attribute should be proceeded first! It's important.

                if (jsonPropertyAttribute != null)
                {
                    entityConfig.SetPropertyAlias(property.Name, jsonPropertyAttribute.PropertyName);
                }

                foreach (var attribute in attributes)
                {
                    if (attribute is IgnoreOnPatchingAttribute || attribute is JsonIgnoreAttribute)
                    {
                        entityConfig.SetPropertyIgnored(property.Name);
                    }

                    if (attribute is RequiredOnPatchingAttribute)
                    {
                        entityConfig.SetPropertyRequired(property.Name);
                    }

                    if (attribute is ConfiguredPatchingAttribute)
                    {
                        entityConfig.SetPropertyUsedInternalConfig(property.Name);
                    }

                    if (attribute is PatchingKeyAttribute keyPropertyAttribute)
                    {
                        if (keyPropertyAttributeFound)
                        {
                            throw new MultipleKeyException();
                        }

                        entityConfig.SetKeyProperty(property.Name, keyPropertyAttribute.Strict);
                        keyPropertyAttributeFound = true;
                    }
                }
            }

            return entityConfig;
        }

        private void ValidateConfig(EntityConfig config)
        {
            var configuredProperties = config.ConfiguredProperties;
            var keyProperties = configuredProperties.Where(x => x.Key);

            if (keyProperties.Count() > 1)
            {
                throw new InvalidKeyException("Multiple key is not supported");
            }

            var keyProperty = keyProperties.FirstOrDefault();
            var requiredPropertyNames = configuredProperties.Where(x => x.Required).Select(x => x.Name);

            if (keyProperty != null)
            {
                if (requiredPropertyNames.Contains(keyProperty.Name))
                {
                    throw new InvalidConfigurationException("Key property can't be required", keyProperty.Name);
                }
            }

            var requiredAndIgnoredIntersect = requiredPropertyNames.Intersect(configuredProperties.Where(x => x.Ignored).Select(x => x.Name)).Any();

            if (requiredAndIgnoredIntersect)
            {
                throw new InvalidConfigurationException("Properties can't be required and ignored at the same time");
            }
        }
    }
}
