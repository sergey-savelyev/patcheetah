using NUnit.Framework;
using Patcheetah.SystemText;
using Patcheetah.Tests.Helpers;
using Patcheetah.Tests.Models.WithAttributes;
using System;

namespace Patcheetah.Tests.SystemText
{
    public class STJ_MultiConfigurationApproachTests : MultiConfigurationApproachTests
    {
        protected override IPatchRequestProvider<User> GetRequestProvider()
        {
            return new SystemTextPatchRequestProvider<User>();
        }

        protected override void Setup()
        {
            PatchEngine.Init(Configure);
        }

        [Test]
        public void SimpleInitEntityCreationTest()
        {
            PatchEngineCore.Cleanup();
            PatchEngine.Init();

            var request = GetPatchRequestWithFields("Login", "LastSeenFrom");
            var entity = request.CreateNewEntity();

            Assert.NotNull(entity);
            Assert.AreEqual(request["Login"].ToString(), entity.Login);
        }
    }
}
