using Patcheetah.Configuration;
using Patcheetah.Patching;
using System;

namespace Patcheetah
{
    public static class PatchEngine
    {
        internal static PatcheetahConfig Config { get; private set; }

        internal static IEntityPatcher Patcher { get; private set; }

        static PatchEngine()
        {
            Config = new PatcheetahConfig();
            Patcher = new DefaultEntityPatcher();
        }

        public static void SetCustomEntityPatcher(IEntityPatcher patcher)
        {
            Patcher = patcher;
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
