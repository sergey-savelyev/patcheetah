using System.Linq;
using Patcheetah.Exceptions;
using Patcheetah.Tests.Models.WithAttributes;
using NUnit.Framework;
using Patcheetah.Mapping;
using Patcheetah.Configuration;

namespace Patcheetah.Tests
{
    public abstract class MultiConfigurationApproachTests : TestBase<User>
    {
        protected void Configure(PatcheetahConfig config)
        {
            config.EnableNestedPatching();
            config.EnableAttributes();
            config.SetPrePatchProcessingFunction((oldVal, newVal, entityToPatch, config) =>
            {
                if (newVal is int age && age == 0)
                {
                    return 18;
                }

                return newVal;
            });
            config
                .ConfigureEntity<User>()
                .UseMapping(x => x.Username, x =>
                {
                    if (x == "convertme")
                    {
                        return MappingResult.MapTo($"{x}_1");
                    }

                    return MappingResult.Skip(x);
                })
                // .Required(x => x.LastSeenFrom) -> we don't need this anymore. Instead of method setup, we'll install it from attribute
                // .IgnoreOnPatching(x => x.Username) -> Same situation, let's install it from attribute
                .SetKey(x => x.Username); // Set login as key but replace it by id from attributes
        }

        [Test]
        public void CallbacksTest()
        {
            var request = GetPatchRequestWithFields("Id", "Username", "Age", "LastSeenFrom");
            var model = new User
            {
                Username = "qwerty",
                Age = 12
            };

            request["Age"] = 0;
            var nick = "convertme";
            request["Username"] = nick;

            request.ApplyTo(model);

            Assert.AreEqual(18, model.Age);
            Assert.AreEqual($"{nick}_1", model.Username);
        }

        [Test]
        public void NestedPatchingTest()
        {
            var request = GetPatchRequestWithFields("Id", "Personal", "LastSeenFrom");
            var model = new User
            {
                PersonalInfo = new Models.PersonalInfo
                {
                    FirstName = "Sergey",
                    LastName = "Qwerty",
                    Birthday = new System.DateTime(1990, 7, 15)
                }
            };

            request.ApplyTo(model);

            Assert.AreEqual("Savelyev", model.PersonalInfo.LastName);
            Assert.AreEqual(new System.DateTime(1990, 7, 15), model.PersonalInfo.Birthday);

            // request["Personal"] as  = null;
        }

        [Test]
        public void OverridingTest()
        {
            // required prop overriding

            var request = GetPatchRequestWithFields("Id");
            var model = new User();
            var requiredException = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var newModel = request.CreateNewEntity();
            });

            Assert.AreEqual("LastSeenFrom", requiredException.Properties.First());

            // ignore prop overriding

            request = GetPatchRequestWithFields("LastSeenFrom", "Login");
            var requestLogin = request["Login"].ToString();
            var modelLogin = "RandomPerson";
            model.Login = modelLogin;

            Assert.AreNotEqual(modelLogin, requestLogin);

            request.ApplyTo(model);

            Assert.AreEqual(modelLogin, model.Login);

            // key overriding test

            KeyTest("Id");
        }
    }
}
