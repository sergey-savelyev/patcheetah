using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Patcheetah.Attributes;
using Patcheetah.Exceptions;

namespace Patcheetah.Configuration
{
    internal class ConfigurationContainer
    {
        private static ConfigurationContainer _container;

        public static ConfigurationContainer Instance => _container ?? (_container = new ConfigurationContainer());

        private IDictionary<string, EntityConfig> _configs;
        private List<IConfigurationBehaviour> _configurationBehaviours;

        private ConfigurationContainer()
        {
            _configs = new Dictionary<string, EntityConfig>();
            _configurationBehaviours = new List<IConfigurationBehaviour>();
        }

        public void RegisterBehaviour(IConfigurationBehaviour behaviour)
        {
            if (_configurationBehaviours.Any(x => x.Id == behaviour.Id))
            {
                throw new Exception($"Configuration behaviour with id {behaviour.Id} already added");
            }

            _configurationBehaviours.Add(behaviour);
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

            // it has parameter equality comparison inside, so if config just created, it don't set again
            entityConfig.SetCaseSensitive(caseSensitive);

            var properties = type.GetProperties();
            var keyPropertyAttributeFound = false;

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is JsonAliasAttribute aliasAttribute)
                    {
                        entityConfig.UseJsonAlias(property.Name, aliasAttribute.Alias);
                    }

                    if (attribute is IgnoreOnPatchingAttribute)
                    {
                        entityConfig.IgnoreOnPatching(property.Name);
                    }

                    if (attribute is RequiredOnPatchingAttribute)
                    {
                        entityConfig.Required(property.Name);
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

                foreach (var behaviour in _configurationBehaviours)
                {
                    behaviour.Configure(property, attributes, entityConfig);
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
