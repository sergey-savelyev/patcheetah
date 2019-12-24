using System;
using System.Linq.Expressions;
using Modelee.Collections;

namespace Modelee.Configuration
{
    public class EntityConfigTyped<TEntity> : EntityConfig
    {
        internal EntityConfigTyped(bool caseSensitive)
            : base(caseSensitive, typeof(TEntity))
        {
        }

        public EntityConfigTyped<TEntity> IgnoreOnPatching<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            IgnoreOnPatching(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> Required<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            Required(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> NotIncludedInViewModel<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            NotIncludedInViewModel(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> AliasInViewModel<TReturn>(Expression<Func<TEntity, TReturn>> property, string alias)
        {
            var propName = ExtractPropertyName(property);
            AliasInViewModel(propName, alias);

            return this;
        }

        public EntityConfigTyped<TEntity> UseModeleeConfig<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            UseModeleeConfig(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> BeforePatch<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Action<PropertyChangedEventArgs> callback)
        {
            BeforePatchCallbacks.RewriteIfExist(ExtractPropertyName(property), callback);

            return this;
        }

        public EntityConfigTyped<TEntity> AfterPatch<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Action<PropertyChangedEventArgs> callback)
        {
            AfterPatchCallbacks.RewriteIfExist(ExtractPropertyName(property), callback);

            return this;
        }

        public void Register()
        {
            ConfigurationContainer.Instance.RegisterConfig<TEntity>(this);
        }

        public void Register<TReturn>(Expression<Func<TEntity, TReturn>> keyPproperty, bool strict = false)
        {
            Register(ExtractPropertyName(keyPproperty), strict);
        }

        public void Register(string keyPropertyName, bool strict)
        {
            SetKeyProperty(keyPropertyName, strict);

            ConfigurationContainer.Instance.RegisterConfig<TEntity>(this);
        }

        private string ExtractPropertyName<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            if (!(property.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Only properties can be patched", property.Name);
            }

            return memberExpression.Member.Name;
        }

    }
}
