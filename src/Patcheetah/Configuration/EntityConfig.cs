using System;
using System.Collections.Generic;
using System.Linq;

namespace Patcheetah.Configuration
{
    public class EntityConfig
    {
        private string _keyPropertyName = string.Empty;
        private IDictionary<string, PropertyConfiguration> _configuredProperties;

        public Type EntityType { get; internal set; }

        public bool CheckKeyOnPatching { get; internal set; }

        public bool CaseSensitive { get; private set; }

        public string KeyPropertyName
        {
            get
            {
                if (string.IsNullOrEmpty(_keyPropertyName))
                {
                    _keyPropertyName = ConfiguredProperties.FirstOrDefault(x => x.IsKey)?.Name;
                }

                return _keyPropertyName;
            }
        }

        public string EntityTypeName { get; }

        public HashSet<PropertyConfiguration> ConfiguredProperties => _configuredProperties.Select(x => x.Value).ToHashSet();

        public PropertyConfiguration this[string propName]
        {
            get
            {
                if (!_configuredProperties.ContainsKey(propName))
                {
                    return null;
                }

                return _configuredProperties[propName];
            }
        }

        internal EntityConfig(bool caseSensitive, Type type)
        {
            CaseSensitive = caseSensitive;
            EntityTypeName = type.Name;
            var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            _configuredProperties = new Dictionary<string, PropertyConfiguration>(stringComparer);
        }

        public void IgnoreOnPatching(string propName)
        {
            CreateAndGetPropertyConfiguration(propName).Ignored = true;
        }

        public void UseJsonAlias(string propName, string alias)
        {
            CreateAndGetPropertyConfiguration(propName).Name = alias;
        }

        public void Required(string propName)
        {
            CreateAndGetPropertyConfiguration(propName).Required = true;
        }

        public void SetKey(string keyProperty, bool checkOnPatching = false)
        {
            if (!string.IsNullOrEmpty(KeyPropertyName))
            {
                CreateAndGetPropertyConfiguration(KeyPropertyName).IsKey = false;
            }

            CheckKeyOnPatching = checkOnPatching;
            CreateAndGetPropertyConfiguration(keyProperty).IsKey = true;
        }

        internal void SetCaseSensitive(bool caseSensitive)
        {
            if (caseSensitive == this.CaseSensitive)
            {
                return;
            }

            var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            var configuredPropertiesCopy = new KeyValuePair<string, PropertyConfiguration>[_configuredProperties.Count];

            _configuredProperties.CopyTo(configuredPropertiesCopy, 0);
            _configuredProperties = new Dictionary<string, PropertyConfiguration>(configuredPropertiesCopy, stringComparer);

            CaseSensitive = caseSensitive;
        }

        public IEnumerator<PropertyConfiguration> GetEnumerator()
        {
            return _configuredProperties.Select(x => x.Value).GetEnumerator();
        }

        internal PropertyConfiguration CreateAndGetPropertyConfiguration(string propertyName)
        {
            if (!_configuredProperties.ContainsKey(propertyName))
            {
                var configuration = new PropertyConfiguration(propertyName);
                _configuredProperties.Add(propertyName, configuration);
            }

            return _configuredProperties[propertyName];
        }
    }
}
