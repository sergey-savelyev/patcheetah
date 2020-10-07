using Patcheetah.JsonNET;
using Patcheetah.Tests.Helpers;
using Patcheetah.Tests.Models.WithAttributes;

namespace Patcheetah.Tests.JsonNet
{
    public class NSJ_SetupFromAttributesTests : SetupFromAttributesTests
    {
        protected override IPatchRequestProvider<User> GetRequestProvider()
        {
            return new NewtonsoftJsonPatchRequestProvider<User>();
        }

        protected override void Setup()
        {
            PatchEngine.Init(Configure);
        }
    }
}
