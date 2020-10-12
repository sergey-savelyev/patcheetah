using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using Patcheetah.Patching;
using System;

namespace Patcheetah
{
    public static class PatchEngineCore
    {
        private static PatcheetahConfig _config;

        private static IJsonTypesResolver _jsonTypesResolver;

        internal static PatcheetahConfig Config => _config ?? throw new UninitializedException();

        internal static IJsonTypesResolver JsonTypesResolver => _jsonTypesResolver ?? throw new UninitializedException();

        public static void Init(Action<PatcheetahConfig> configure, IJsonTypesResolver jsonTypesResolver)
        {
            _jsonTypesResolver = jsonTypesResolver;
            var config = new PatcheetahConfig();

            configure(config);
            _config = config;
        }

        public static void Cleanup()
        {
            _config?.Cleanup();
        }
    }
}
