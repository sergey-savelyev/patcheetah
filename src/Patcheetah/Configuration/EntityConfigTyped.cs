using System;
using System.Linq.Expressions;
using Patcheetah.Mapping;
using Patcheetah.Patching;

namespace Patcheetah.Configuration
{
    public class EntityConfigTyped<TEntity> : EntityConfig
    {
        internal EntityConfigTyped(bool caseSensitive)
            : base(caseSensitive, typeof(TEntity))
        {
        }

        public EntityConfigTyped<TEntity> UseMapping<TReturn>(Expression<Func<TEntity, TReturn>> property, Func<TReturn, MappingResultTyped<TReturn>> mappingHandler)
        {
            var handler = new MappingHandler(obj => mappingHandler((TReturn)obj));
            var propName = ExtractPropertyName(property);

            UseMapping(propName, handler);

            return this;
        }

        public EntityConfigTyped<TEntity> IgnoreOnPatching<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            IgnoreOnPatching(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> UseJsonAlias<TReturn>(Expression<Func<TEntity, TReturn>> property, string alias)
        {
            var propName = ExtractPropertyName(property);
            UseJsonAlias(propName, alias);

            return this;
        }

        public EntityConfigTyped<TEntity> Required<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            Required(propName);

            return this;
        }

        public EntityConfigTyped<TEntity> BeforePatch<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Action<PropertyChangedEventArgs> callback)
        {
            SetPropertyBeforePatchCallback(ExtractPropertyName(property), callback);

            return this;
        }

        public EntityConfigTyped<TEntity> AfterPatch<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Action<PropertyChangedEventArgs> callback)
        {
            SetPropertyAfterPatchCallback(ExtractPropertyName(property), callback);

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
