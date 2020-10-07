using Patcheetah.SystemText;
using Patcheetah.Tests.Helpers;
using Patcheetah.Tests.Models.Standard;

namespace Patcheetah.Tests.SystemText
{
    public class STJ_SetupFromConfigTests : SetupFromConfigTests
    {
        protected override IPatchRequestProvider<User> GetRequestProvider()
        {
            return new SystemTextPatchRequestProvider<User>();
        }

        protected override void Setup()
        {
            PatchEngine.Init(Configure);
        }
    }
}
