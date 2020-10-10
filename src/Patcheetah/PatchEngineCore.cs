using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using Patcheetah.Patching;
using System;
using System.Linq;

namespace Patcheetah
{
    public static class PatchEngineCore
    {
        private static PatcheetahConfig _config;
        private static EntityPatcher _patcher;

        internal static PatcheetahConfig Config => _config ?? throw new UninitializedException();

        internal static EntityPatcher Patcher => _patcher ?? throw new UninitializedException();

        public static void Init(Action<PatcheetahConfig> configure, IJsonTypesResolver jsonTypesResolver)
        {
            var config = new PatcheetahConfig();
            var patcher = new EntityPatcher(jsonTypesResolver);

            configure(config);
            _config = config;
            _patcher = patcher;
        }

        public static void Cleanup()
        {
            _config?.Cleanup();
        }
    }
}
