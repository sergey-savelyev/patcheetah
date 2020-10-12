using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Patcheetah.Attributes;
using Patcheetah.Exceptions;
using Patcheetah.Mapping;
using Patcheetah.Patching;

namespace Patcheetah.Configuration
{
    public class PatcheetahConfig
    {
        private ConcurrentDictionary<int, EntityConfig> _configs;
        private ICustomAttributesConfigurator _customAttributesConfigurator;
        private bool _attributesEnabled = false;
        private ConcurrentDictionary<int, MappingHandler> _handlers;

        internal MappingHandler GlobalMappingHandler { get; private set; }

        internal Func<PatchContext, object> PrePatchProcessingFunc { get; private set; }

        internal bool RFC7396Enabled { get; private set; }

        internal bool GlobalCaseSensitivity { get; private set; }

        public PatcheetahConfig()
        {
            _configs = new ConcurrentDictionary<int, EntityConfig>();
            _handlers = new ConcurrentDictionary<int, MappingHandler>();
        }

        public EntityConfigurator<TEntity> ConfigureEntity<TEntity>(bool caseSensitive = false)
        {
            var entityConfig = new EntityConfig(caseSensitive, typeof(TEntity));
            RegisterConfigForType(entityConfig, typeof(TEntity));

            return new EntityConfigurator<TEntity>(entityConfig);
        }

        public void EnableNestedPatching()
        {
            RFC7396Enabled = true;
        }

        public void EnableAttributes()
        {
            _attributesEnabled = true;
        }
        
        public void SetCaseSensitivity(bool caseSensitive)
        {
            GlobalCaseSensitivity = caseSensitive;
        }

        public void SetMappingForType<T>(Func<T, MappingResultTypedWrapper<T>> mapHandler)
        {
            var handler = new MappingHandler(obj => mapHandler((T)obj));
            var typeHash = typeof(T).GetHashCode();

            if (!_handlers.ContainsKey(typeHash))
            {
                _handlers.TryAdd(typeHash, handler);
            }

            _handlers[typeHash] = handler;
        }

        public void SetGlobalMapping(Func<object, MappingResult> mapHandler)
        {
            GlobalMappingHandler = new MappingHandler(obj => mapHandler(obj));
        }

        public void SetPrePatchProcessingFunction(Func<PatchContext, object> prePatchProcessingFunction)
        {
            PrePatchProcessingFunc = prePatchProcessingFunction;
        }

        public void SetCustomAttributesConfigurator(ICustomAttributesConfigurator configurator)
        {
            _customAttributesConfigurator = configurator;
        }

        internal EntityConfig GetConfig<TEntity>()
        {
            var type = typeof(TEntity);

            return GetEntityConfig(type);
        }

        internal MappingHandler GetMappingHandlerForType(Type type)
        {
            var thash = type.GetHashCode();

            if (_handlers.ContainsKey(thash))
            {
                return _handlers[thash];
            }

            return null;
        }

        internal void Cleanup()
        {
            _configs.Clear();
            _attributesEnabled = false;
            RFC7396Enabled = false;
        }

        internal EntityConfig GetEntityConfig(Type type)
        {
            var thash = type.GetHashCode();

            if (!_configs.ContainsKey(thash))
            {
                if (_attributesEnabled)
                {
                    RegisterConfigForType(null, type);

                    return GetEntityConfig(type);
                }

                return null;
            }

            return _configs[thash];
        }

        private void RegisterConfigForType(EntityConfig config, Type type)
        {
            var thash = type.GetHashCode();

            if (_attributesEnabled)
            {
                config = ObtainEntityConfigFromType(type, config);
            }

            ValidateEntityConfig(config);

            if (_configs.ContainsKey(thash))
            {
                _configs.TryRemove(thash, out _);
            }

            _configs.TryAdd(thash, config);
        }

        private EntityConfig ObtainEntityConfigFromType(Type type, EntityConfig entityConfig = null)
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

            entityConfig.EntityType = type;

            // it has parameter equality comparison inside, so if config just created, it doesn't set again
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
                        entityConfig.GetOrCreatePropertyConfiguration(property.Name).Name = aliasAttribute.Alias;
                    }

                    if (attribute is IgnoreOnPatchingAttribute)
                    {
                        entityConfig.GetOrCreatePropertyConfiguration(property.Name).Ignored = true;
                    }

                    if (attribute is RequiredOnPatchingAttribute)
                    {
                        entityConfig.GetOrCreatePropertyConfiguration(property.Name).Required = true;
                    }

                    if (attribute is PatchingKeyAttribute keyPropertyAttribute)
                    {
                        if (keyPropertyAttributeFound)
                        {
                            throw new MultipleKeyException();
                        }

                        entityConfig.SetKey(property.Name, keyPropertyAttribute.Strict);
                        keyPropertyAttributeFound = true;
                    }
                }

                _customAttributesConfigurator?.Configure(property, attributes, new EntityConfigAccessor(entityConfig));
            }

            return entityConfig;
        }

        private static void ValidateEntityConfig(EntityConfig config)
        {
            var configuredProperties = config.ConfiguredProperties;
            var keyProperties = configuredProperties.Where(x => x.IsKey);

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
