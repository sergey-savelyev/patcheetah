using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Patcheetah.Configuration;

namespace Patcheetah.Patching
{
    public class PatchObject<TEntity> : IDictionary<string, object>
        where TEntity : class, new()
    {
        private readonly Dictionary<string, object> _patchProperties;
        private readonly EntityConfig _entityConfig;

        public bool HasKey => string.IsNullOrEmpty(_entityConfig.KeyPropertyName) ?
            true :
            !_patchProperties.ContainsKey(_entityConfig.KeyPropertyName);

        public object Key => HasKey ? null : _patchProperties[_entityConfig.KeyPropertyName];

        // IDictionary properties
        public int Count => _patchProperties.Count;
        public bool IsReadOnly => ((IDictionary<string, object>)_patchProperties).IsReadOnly;
        public ICollection<string> Keys => _patchProperties.Keys;
        public ICollection<object> Values => _patchProperties.Values;

        public PatchObject()
        {
            _entityConfig = ConfigurationContainer.Instance.GetConfig<TEntity>();
            _patchProperties = new Dictionary<string, object>(_entityConfig?.CaseSensitive ?? false ?
                StringComparer.Ordinal :
                StringComparer.OrdinalIgnoreCase);
        }

        public void Patch(TEntity entity)
        {
            EntityBuilder<TEntity>.PatchEntity(entity, _patchProperties, _entityConfig);
        }

        public TEntity CreateEntity()
        {
            return EntityBuilder<TEntity>.BuildEntity(_patchProperties, _entityConfig);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _patchProperties.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _patchProperties.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _patchProperties.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _patchProperties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_patchProperties).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _patchProperties.Remove(item.Key);
        }

        public void Add(string key, object value)
        {
            _patchProperties.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _patchProperties.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _patchProperties.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _patchProperties.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => _patchProperties[key];
            set { _patchProperties[key] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}