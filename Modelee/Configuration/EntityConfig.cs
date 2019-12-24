﻿using System;
using System.Collections.Generic;
using System.Linq;
using Modelee.Collections;

namespace Modelee.Configuration
{
    public class EntityConfig
    {
        private bool _simplePatchingComputed;
        private bool _simplePatchingUsed;

        internal CaseSensitiveList IgnoredProperties { get; private set; }
        internal CaseSensitiveList RequiredProperties { get; private set; }
        internal CaseSensitiveList NotIncludedProperies { get; private set; }
        internal CaseSensitiveList UseModeleeConfigProperties { get; private set; }
        internal IDictionary<string, string> ViewModelAliasProperties { get; private set; }
        internal IDictionary<string, Action<PropertyChangedEventArgs>> BeforePatchCallbacks { get; private set; }
        internal IDictionary<string, Action<PropertyChangedEventArgs>> AfterPatchCallbacks { get; private set; }

        public string KeyPropertyName { get; private set; }

        public bool StrictKeyUsed { get; private set; }

        public bool CaseSensitive { get; private set; }

        public string EntityTypeName { get; }

        public bool SimplePatchingUsed
        {
            get
            {
                if (!_simplePatchingComputed)
                {
                    _simplePatchingUsed = ComputeSimplePatching();
                    _simplePatchingComputed = true;
                }

                return _simplePatchingUsed;
            }
        }
            

        internal EntityConfig(bool caseSensitive, Type entityType)
        {
            CaseSensitive = caseSensitive;
            EntityTypeName = entityType.Name;
            var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            IgnoredProperties = new CaseSensitiveList(caseSensitive);
            RequiredProperties = new CaseSensitiveList(caseSensitive);
            NotIncludedProperies = new CaseSensitiveList(caseSensitive);
            UseModeleeConfigProperties = new CaseSensitiveList(caseSensitive);
            ViewModelAliasProperties = new Dictionary<string, string>(stringComparer);
            BeforePatchCallbacks = new Dictionary<string, Action<PropertyChangedEventArgs>>(stringComparer);
            AfterPatchCallbacks = new Dictionary<string, Action<PropertyChangedEventArgs>>(stringComparer);
        }

        internal void IgnoreOnPatching(string propertyName)
        {
            IgnoredProperties.AddIfNotExist(GetPropertyAlias(propertyName));
        }

        internal void Required(string propertyName)
        {
            RequiredProperties.AddIfNotExist(GetPropertyAlias(propertyName));
        }

        internal void NotIncludedInViewModel(string propertyName)
        {
            NotIncludedProperies.AddIfNotExist(propertyName);
        }

        internal void AliasInViewModel(string propertyName, string alias)
        {
            var prevPropName = propertyName;

            if (ViewModelAliasProperties.ContainsKey(propertyName))
            {
                prevPropName = ViewModelAliasProperties[propertyName];
            }

            var ignored = IgnoredProperties.Remove(prevPropName);
            var required = RequiredProperties.Remove(prevPropName);
            var usingModelee = UseModeleeConfigProperties.Remove(prevPropName);

            if (ignored)
            {
                IgnoredProperties.Add(alias);
            }

            if (required)
            {
                RequiredProperties.Add(alias);
            }

            if (usingModelee)
            {
                UseModeleeConfigProperties.Add(alias);
            }

            ViewModelAliasProperties.RewriteIfExist(propertyName, alias);
        }

        internal void UseModeleeConfig(string propertyName)
        {
            UseModeleeConfigProperties.AddIfNotExist(GetPropertyAlias(propertyName));
        }

        internal void SetCaseSensitive(bool caseSensitive)
        {
            if (caseSensitive == this.CaseSensitive)
            {
                return;
            }

            IgnoredProperties.SetCaseSensitivity(caseSensitive);
            RequiredProperties.SetCaseSensitivity(caseSensitive);
            NotIncludedProperies.SetCaseSensitivity(caseSensitive);
            UseModeleeConfigProperties.SetCaseSensitivity(caseSensitive);

            var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            var viewModelAliasProperties = new KeyValuePair<string, string>[ViewModelAliasProperties.Count];
            var beforePatchCallbacks = new KeyValuePair<string, Action<PropertyChangedEventArgs>>[BeforePatchCallbacks.Count];
            var afterPatchCallbacks = new KeyValuePair<string, Action<PropertyChangedEventArgs>>[AfterPatchCallbacks.Count];

            ViewModelAliasProperties.CopyTo(viewModelAliasProperties, 0);
            ViewModelAliasProperties = new Dictionary<string, string>(viewModelAliasProperties, stringComparer);

            BeforePatchCallbacks.CopyTo(beforePatchCallbacks, 0);
            BeforePatchCallbacks = new Dictionary<string, Action<PropertyChangedEventArgs>>(beforePatchCallbacks, stringComparer);

            AfterPatchCallbacks.CopyTo(afterPatchCallbacks, 0);
            AfterPatchCallbacks = new Dictionary<string, Action<PropertyChangedEventArgs>>(afterPatchCallbacks, stringComparer);

            CaseSensitive = caseSensitive;
        }

        internal void SetKeyProperty(string propertyName, bool strict)
        {
            StrictKeyUsed = strict;
            KeyPropertyName = GetPropertyAlias(propertyName);
        }

        private string GetPropertyAlias(string propertyName)
        {
            var alias = propertyName;

            if (ViewModelAliasProperties.ContainsKey(propertyName))
            {
                alias = ViewModelAliasProperties[propertyName];
            }

            return alias;
        }

        private bool ComputeSimplePatching()
        {
            return !IgnoredProperties.Any() &&
                !RequiredProperties.Any() &&
                !NotIncludedProperies.Any() &&
                !UseModeleeConfigProperties.Any() &&
                !ViewModelAliasProperties.Any() &&
                !BeforePatchCallbacks.Any() &&
                !AfterPatchCallbacks.Any();
        }
    }
}
