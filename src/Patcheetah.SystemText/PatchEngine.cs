using Patcheetah.Configuration;
using System;

namespace Patcheetah.SystemText
{
    public class PatchEngine
    {
        public static void Init()
        {
            Init(x => { });
        }

        public static void Init(Action<PatcheetahConfig> configure)
        {
            PatchEngineCore.Init(configure, new SystemTextJsonTypesResolver());
        }
    }
}
