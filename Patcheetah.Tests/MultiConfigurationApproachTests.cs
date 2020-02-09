using System.Linq;
using Patcheetah.Configuration;
using Patcheetah.Exceptions;
using Patcheetah.Tests.Models.WithAttributes;
using NUnit.Framework;

namespace Patcheetah.Tests
{
    public class MultiConfigurationApproachTests : TestBase<User, PersonalInfo, Contact, UserAddress>
    {
        protected override void Setup()
        {
            PatcheetahConfig.CreateFor<User>()
                // .Required(x => x.LastSeenFrom) -> instead of method setup, we'll install it from attribute
                // .IgnoreOnPatching(x => x.Username) -> Same situation, let's install it from attribute
                .Register(x => x.NickName); // Set name as key but replace it by id from attributes
        }

        [Test]
        public void OverridingTest()
        {
            // required prop overriding

            var request = GetPatchRequestWithFields("Id");
            var model = new User();
            var requiredException = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var newModel = request.CreateEntity();
            });

            Assert.AreEqual("LastSeenFrom", requiredException.Properties.First());

            // ignore prop overriding

            request = GetPatchRequestWithFields("LastSeenFrom", "Username");
            var requestUsername = request["Username"].ToObject<string>();
            var modelUsername = "RandomPerson";
            model.Username = modelUsername;

            Assert.AreNotEqual(modelUsername, requestUsername);

            request.Patch(model);

            Assert.AreEqual(modelUsername, model.Username);

            // key overriding test

            KeyTest();
        }
    }
}
