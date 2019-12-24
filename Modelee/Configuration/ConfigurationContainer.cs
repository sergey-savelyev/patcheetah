using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modelee.Attributes;
using Modelee.Exceptions;

namespace Modelee.Configuration
{
    internal class ConfigurationContainer
    {
        private static ConfigurationContainer _container;

        internal static ConfigurationContainer Instance => _container ?? (_container = new ConfigurationContainer());

        private IDictionary<string, EntityConfig> _configs;

        private ConfigurationContainer()
        {
            _configs = new Dictionary<string, EntityConfig>();
        }

        internal void RegisterConfig<TEntity>(EntityConfig config)
        {
            this.RegisterConfigForType(config, typeof(TEntity));
        }

        internal EntityConfig GetConfig<TEntity>()
        {
            var type = typeof(TEntity);

            return GetConfig(type);
        }

        internal EntityConfig GetConfig(Type type)
        {
            var name = type.Name;

            if (!_configs.ContainsKey(name))
            {
                var config = ObtainConfigFromType(type);
                RegisterConfigForType(config, type);

                return config;
            }

            return _configs[name];
        }

        private void RegisterConfigForType(EntityConfig config, Type type)
        {
            var name = type.Name;
            config = ObtainConfigFromType(type, config);
            _configs.Add(name, config);
        }

        private EntityConfig ObtainConfigFromType(Type type, EntityConfig entityConfig = null)
        {
            var caseSensitive = false;
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
                var aliasAttribute = property.GetCustomAttribute<ViewModelNameAttribute>();

                // Alias attribute should be proceeded first! It's important.
                if (aliasAttribute != null)
                {
                    entityConfig.AliasInViewModel(property.Name, aliasAttribute.Name);
                }

                foreach (var attribute in attributes)
                {
                    if (attribute is IgnoreOnPatchingAttribute)
                    {
                        entityConfig.IgnoreOnPatching(property.Name);
                    }

                    if (attribute is RequiredFieldAttribute)
                    {
                        entityConfig.Required(property.Name);
                    }

                    if (attribute is NotIncludedInViewModelAttribute)
                    {
                        entityConfig.NotIncludedInViewModel(property.Name);
                    }

                    if (attribute is UseModeleeAttribute)
                    {
                        entityConfig.UseModeleeConfig(property.Name);
                    }

                    if (attribute is KeyPropertyAttribute keyPropertyAttribute)
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
    }
}
