using System;
using System.Collections.Generic;
using System.Linq;
using Patcheetah.Exceptions;
using Patcheetah.Tests.Models;
using Patcheetah.Tests.Models.Abstract;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Patcheetah.Patching;

namespace Patcheetah.Tests
{
    public abstract class TestBase<TUser>
        where TUser : class, IUser, new()
    {
        [OneTimeSetUp]
        public void SetupInternal()
        {
            PatchEngine.Reset();
            Setup();
        }

        [Test]
        public void RequiredTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var request = GetPatchRequestWithFields("Id", "LastSeenFrom");
                var entity = request.CreateEntity();
            });

            var exception = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var request = GetPatchRequestWithFields("Id");
                var entity = request.CreateEntity();
            });

            Assert.That(exception.Properties.First(), Is.EqualTo("LastSeenFrom"));
        }

        [Test]
        public void IgnoredTest()
        {
            var model = new TUser
            {
                Id = Guid.NewGuid().ToString(),
                Login = "Testee",
            };

            var loginValue = model.Login;
            var request = GetPatchRequestWithFields("Id", "Login", "LastSeenFrom");

            request.Patch(model);

            Assert.AreEqual(model.Login, loginValue);
        }

        [Test]
        public void WrongTypeTest()
        {
            var request = GetPatchRequestWithFields("LastSeenFrom", "AccessRights");
            request["AccessRights"] = new JArray
            {
                JObject.FromObject(new { Foo = "foo" }),
                JObject.FromObject(new { Bar = "bar" }),
            };

            var model = new TUser
            {
                AccessRights = new[] { "Full" }
            };

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                request.Patch(model);
            });
        }

        [Test]
        public void NullPatchingTest()
        {
            // property with modelee config
            var request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Personal", null);
            var model = new TUser
            {
                PersonalInfo = new PersonalInfo
                {
                    Birthday = new DateTime(1987, 02, 02),
                    FirstName = "Random",
                    LastName = "Person"
                },
                Contacts = new List<Contact>
                {
                    new Contact
                    {
                        Type = "Email",
                        Value = "random@mail.test"
                    }
                },
            };

            request.Patch(model);

            Assert.AreEqual(null, model.PersonalInfo);

            // property with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Username", null);
            request.Patch(model);

            Assert.AreEqual(null, model.Username);

            // array with modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Contacts", null);
            request.Patch(model);

            Assert.AreEqual(null, model.Contacts);

            // array with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Personal", null);
            request.Patch(model);

            Assert.AreEqual(null, model.PersonalInfo);
        }

        protected void KeyTest(string key)
        {
            var requestWithKey = GetPatchRequestWithFields(key, "LastSeenFrom");
            Assert.IsTrue(requestWithKey.HasKey);

            var requestWithoutKey = GetPatchRequestWithFields("LastSeenFrom");
            Assert.IsFalse(requestWithoutKey.HasKey);
        }

        protected void CaseSensitiveTest(bool sensitive)
        {
            var request = GetPatchRequestWithFields("LastSeenFrom", "Username");
            var usernameValue = request["Username"].ToString();
            request.Remove("Username", out _);
            request.Add("username", usernameValue);

            var modelUsername = "Testee";
            var model = new TUser
            {
                Username = modelUsername
            };

            request.Patch(model);

            if (sensitive)
            {
                Assert.AreNotEqual(usernameValue, model.Username);

                return;
            }

            Assert.AreEqual(usernameValue, model.Username);
        }

        protected abstract void Setup();

        protected PatchObject<TUser> GetPatchRequestWithFields(params string[] fieldNames)
        {
            return PatchRequestsConstructor.GetRequestWithFields(fieldNames).ToObject<PatchObject<TUser>>();
        }
    }
}
