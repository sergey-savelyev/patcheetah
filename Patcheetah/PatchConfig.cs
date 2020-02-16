using System;
using Patcheetah.Configuration;
using Patcheetah.Mapping;

namespace Patcheetah
{
    public static class PatchConfig
    {
        public static EntityConfigTyped<TEntity> CreateForEntity<TEntity>(bool caseSensitive = false)
            => new EntityConfigTyped<TEntity>(caseSensitive);

        public static void SetMappingForType<T>(Func<T, MappingResultTyped<T>> mapHandler)
        {
            MappingHandlersContainer.Instance.AddHandler(typeof(T), new MappingHandler(obj => mapHandler((T)obj)));
        }

        public static ExtendedSettingsService ExtendedSettings => new ExtendedSettingsService();
    }
}
