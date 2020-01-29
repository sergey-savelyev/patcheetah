using System;
using Modelee.Mapping;

namespace Modelee.Configuration
{
    public static class ModeleeConfig
    {
        public static EntityConfigTyped<TEntity> CreateFor<TEntity>(bool caseSensitive = false)
            => new EntityConfigTyped<TEntity>(caseSensitive);

        public static void SetMappingForType<T>(Func<T, MappingResultTyped<T>> mapHandler, int priority = 0)
        {
            MappingHandlersContainer.Instance.AddHandler(typeof(T), new MappingHandler(obj => mapHandler((T)obj)));
        }
    }
}
