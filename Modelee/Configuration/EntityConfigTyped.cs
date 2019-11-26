using System;
using System.Linq.Expressions;
using Modelee.Collections;

namespace Modelee.Configuration
{
    public class EntityConfigTyped<TEntity> : EntityConfig
    {
        public EntityConfigTyped(bool caseSensitive)
            : base(caseSensitive)
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

        public void Register<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            Register(ExtractPropertyName(property));
        }

        public void Register(string keyPropertyName)
        {
            KeyPropertyName = keyPropertyName;

            ConfigurationContainer.Instance.RegisterConfig<TEntity>(this);
        }

        private string ExtractPropertyName<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            if (!(property.Body is MemberExpression memberExpression))
            {
                throw new Exception("Only properties can be patched");
            }

            return memberExpression.Member.Name;
        }

    }
}
