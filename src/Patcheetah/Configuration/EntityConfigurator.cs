using Patcheetah.Mapping;
using Patcheetah.Patching;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Patcheetah.Configuration
{
    public class EntityConfigurator<TEntity>
    {
        private EntityConfig _config;

        public EntityConfigurator(EntityConfig config)
        {
            _config = config;
        }

        public EntityConfigurator<TEntity> UseMapping<TReturn>(Expression<Func<TEntity, TReturn>> property, Func<TReturn, MappingResultTypedWrapper<TReturn>> mappingHandler)
        {
            var handler = new MappingHandler(obj => mappingHandler((TReturn)obj));
            var propName = ExtractPropertyName(property);

            _config.CreateAndGetPropertyConfiguration(propName).MappingHandler = handler;

            return this;
        }

        public EntityConfigurator<TEntity> IgnoreOnPatching<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            _config.IgnoreOnPatching(propName);

            return this;
        }

        public EntityConfigurator<TEntity> UseJsonAlias<TReturn>(Expression<Func<TEntity, TReturn>> property, string alias)
        {
            var propName = ExtractPropertyName(property);
            _config.UseJsonAlias(propName, alias);

            return this;
        }

        public EntityConfigurator<TEntity> Required<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);
            _config.Required(propName);

            return this;
        }

        public EntityConfigurator<TEntity> BeforeMapping<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Func<PropertyChangedEventArgs, object> callback)
        {
            var propName = ExtractPropertyName(property);
            _config.CreateAndGetPropertyConfiguration(propName).BeforeMappingCallback = callback;

            return this;
        }

        public EntityConfigurator<TEntity> AfterPatch<TReturn>(
            Expression<Func<TEntity, TReturn>> property,
            Action<PropertyChangedEventArgs> callback)
        {
            var propName = ExtractPropertyName(property);
            _config.CreateAndGetPropertyConfiguration(propName).AfterSetCallback = callback;

            return this;
        }

        public Dictionary<string, object> GetExtraSettingsForProperty<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExtractPropertyName(property);

            return _config.CreateAndGetPropertyConfiguration(propName).ExtraSettings;
        }

        public EntityConfigurator<TEntity> SetKey<TReturn>(Expression<Func<TEntity, TReturn>> keyPproperty, bool checkOnPatching = false)
        {
            var propName = ExtractPropertyName(keyPproperty);
            _config.SetKey(propName, checkOnPatching);

            return this;
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
