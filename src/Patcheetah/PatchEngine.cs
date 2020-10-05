using Patcheetah.Configuration;
using Patcheetah.Patching;
using System;

namespace Patcheetah
{
    public static class PatchEngine
    {
        internal static PatcheetahConfig Config { get; private set; }

        internal static EntityPatcher Patcher { get; private set; }

        static PatchEngine()
        {
            Config = new PatcheetahConfig();
            Patcher = new EntityPatcher();
        }

        public static void Setup(Action<PatcheetahConfig> configure)
        {
            configure(Config);
        }

        public static void Reset()
        {
            Config.Cleanup();
        }
    }
}
