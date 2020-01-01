﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Modelee.Configuration
{
    public class EntityConfig
    {
        private string _keyPropertyName = string.Empty;
        private IDictionary<string, PropertyConfiguration> _configuredProperties;

        public bool CheckKeyOnPatching { get; private set; }

        public bool CaseSensitive { get; private set; }

        public string KeyPropertyName
        {
            get
            {
                if (_keyPropertyName == string.Empty)
                {
                    _keyPropertyName = ConfiguredProperties.FirstOrDefault(x => x.Key)?.Name;
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

        internal void IgnoreOnPatching(string propertyName)
        {
            GetPropertyConfiguration(propertyName).Ignored = true;
        }

        internal void Required(string propertyName)
        {
            GetPropertyConfiguration(propertyName).Required = true;
        }

        internal void NotIncludedInViewModel(string propertyName)
        {
            GetPropertyConfiguration(propertyName).NotIncludedInViewModel = true;
        }

        internal void AliasInViewModel(string propertyName, string alias)
        {
            GetPropertyConfiguration(propertyName).ViewModelAlias = alias;
        }

        internal void UseModeleeConfig(string propertyName)
        {
            GetPropertyConfiguration(propertyName).HasModeleeCongig = true;
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

        internal void SetKeyProperty(string propertyName, bool strict)
        {
            var prevKey = _configuredProperties.FirstOrDefault(x => x.Value.Key);
            if (!string.IsNullOrEmpty(prevKey.Key))
            {
                prevKey.Value.Key = false;
            }

            CheckKeyOnPatching = strict;
            GetPropertyConfiguration(propertyName).Key = true;
        }

        internal void SetBeforePatchCallback(string propertyName, Action<PropertyChangedEventArgs> callback)
        {
            GetPropertyConfiguration(propertyName).BeforePatchCallback = callback;
        }

        internal void SetAfterPatchCallback(string propertyName, Action<PropertyChangedEventArgs> callback)
        {
            GetPropertyConfiguration(propertyName).AfterPatchCallback = callback;
        }

        private PropertyConfiguration GetPropertyConfiguration(string propertyName)
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
