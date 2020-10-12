using Patcheetah.Mapping;
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
            var propName = ExpressionHelper.ExtractPropertyName(property);
            _config.GetOrCreatePropertyConfiguration(propName).MappingHandler = obj => mappingHandler((TReturn)obj);

            return this;
        }

        public EntityConfigurator<TEntity> IgnoreOnPatching<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExpressionHelper.ExtractPropertyName(property);
            _config.IgnoreOnPatching(propName);

            return this;
        }

        public EntityConfigurator<TEntity> UseJsonAlias<TReturn>(Expression<Func<TEntity, TReturn>> property, string alias)
        {
            var propName = ExpressionHelper.ExtractPropertyName(property);
            _config.UseJsonAlias(propName, alias);

            return this;
        }

        public EntityConfigurator<TEntity> Required<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExpressionHelper.ExtractPropertyName(property);
            _config.Required(propName);

            return this;
        }

        public Dictionary<string, object> GetExtraSettingsForProperty<TReturn>(Expression<Func<TEntity, TReturn>> property)
        {
            var propName = ExpressionHelper.ExtractPropertyName(property);

            return _config.GetOrCreatePropertyConfiguration(propName).ExtraSettings;
        }

        public EntityConfigurator<TEntity> SetKey<TReturn>(Expression<Func<TEntity, TReturn>> keyPproperty, bool checkOnPatching = false)
        {
            var propName = ExpressionHelper.ExtractPropertyName(keyPproperty);
            _config.SetKey(propName, checkOnPatching);

            return this;
        }
    }
}
