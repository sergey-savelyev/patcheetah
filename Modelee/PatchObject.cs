using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modelee.Builders;
using Modelee.Configuration;
using Modelee.Exceptions;
using Newtonsoft.Json.Linq;

namespace Modelee
{
    public sealed class PatchObject<TEntity> : IDictionary<string, JToken>
        where TEntity : class, new()
    {
        private readonly Dictionary<string, JToken> _patchProperties;
        private readonly EntityConfig _entityConfig;

        public bool IsNew
        {
            get
            {
                if (string.IsNullOrEmpty(_entityConfig.KeyPropertyName))
                {
                    return true;
                }

                return !_patchProperties.ContainsKey(_entityConfig.KeyPropertyName);
            }
        }

        public JToken Key => IsNew ? null : _patchProperties[_entityConfig.KeyPropertyName];

        // IDictionary properties
        public int Count => _patchProperties.Count;
        public bool IsReadOnly => ((IDictionary<string, JToken>)_patchProperties).IsReadOnly;
        public ICollection<string> Keys => _patchProperties.Keys;
        public ICollection<JToken> Values => _patchProperties.Values;

        public PatchObject()
        {
            _entityConfig = ConfigurationContainer.Instance.GetConfig<TEntity>();
            _patchProperties = new Dictionary<string, JToken>(_entityConfig.CaseSensitive ?
                StringComparer.Ordinal :
                StringComparer.OrdinalIgnoreCase);
        }

        public void Patch(ref TEntity entity)
        {
            entity = ModelBuilder.BuildFrom(this, entity);
        }

        public TEntity Patch(TEntity entity)
        {
            return ModelBuilder.BuildFrom(this, entity);
        }

        public TEntity CreateEntity()
        {
            var model = ModelBuilder.BuildFrom(this);

            return model;
        }

        public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()
        {
            return _patchProperties.GetEnumerator();
        }

        public void Add(KeyValuePair<string, JToken> item)
        {
            _patchProperties.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _patchProperties.Clear();
        }

        public bool Contains(KeyValuePair<string, JToken> item)
        {
            return _patchProperties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, JToken>[] array, int arrayIndex)
        {
            ((IDictionary<string, JToken>)_patchProperties).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, JToken> item)
        {
            return _patchProperties.Remove(item.Key);
        }

        public void Add(string key, JToken value)
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

        public bool TryGetValue(string key, out JToken value)
        {
            return _patchProperties.TryGetValue(key, out value);
        }

        public JToken this[string key]
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