using Patcheetah.Configuration;
using System;

namespace Patcheetah.JsonNET
{
    public static class PatchEngine
    {
        public static void Init()
        {
            Init(x => { });
        }

        public static void Init(Action<PatcheetahConfig> configure)
        {
            PatchEngineCore.Init(cfg => 
            {
                cfg.SetCustomAttributesConfigurator(new NewtonsoftAttributesConfigurator());
                configure(cfg);
            }, new NewtonsoftJsonTypesResolver());
        }
    }
}
