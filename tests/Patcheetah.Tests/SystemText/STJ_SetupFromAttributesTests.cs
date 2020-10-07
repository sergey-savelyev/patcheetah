using Patcheetah.SystemText;
using Patcheetah.Tests.Helpers;
using Patcheetah.Tests.Models.WithAttributes;

namespace Patcheetah.Tests.SystemText
{
    public class STJ_SetupFromAttributesTests : SetupFromAttributesTests
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
