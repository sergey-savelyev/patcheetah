using System;
using System.Collections.Generic;
using System.Linq;
using Patcheetah.Exceptions;
using Patcheetah.Tests.Models;
using Patcheetah.Tests.Models.Abstract;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Patcheetah.Patching;
using Patcheetah.Tests.Helpers;

namespace Patcheetah.Tests
{
    public abstract class TestBase<TUser>
        where TUser : class, IUser, new()
    {
        private IPatchRequestProvider<TUser> _patchRequestProvider;

        [OneTimeSetUp]
        public void SetupInternal()
        {
            PatchEngineCore.Reset();
            _patchRequestProvider = GetRequestProvider();
            Setup();
        }

        [Test]
        public void RequiredTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var request = GetPatchRequestWithFields("Id", "LastSeenFrom");
                var entity = request.CreateNewEntity();
            });

            var exception = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var request = GetPatchRequestWithFields("Id");
                var entity = request.CreateNewEntity();
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

            request.ApplyTo(model);

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
                request.ApplyTo(model);
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

            request.ApplyTo(model);

            Assert.AreEqual(null, model.PersonalInfo);

            // property with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Username", null);
            request.ApplyTo(model);

            Assert.AreEqual(null, model.Username);

            // array with modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Contacts", null);
            request.ApplyTo(model);

            Assert.AreEqual(null, model.Contacts);

            // array with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Personal", null);
            request.ApplyTo(model);

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

            request.ApplyTo(model);

            if (sensitive)
            {
                Assert.AreNotEqual(usernameValue, model.Username);

                return;
            }

            Assert.AreEqual(usernameValue, model.Username);
        }

        protected abstract void Setup();

        protected abstract IPatchRequestProvider<TUser> GetRequestProvider();

        protected PatchObject<TUser> GetPatchRequestWithFields(params string[] fieldNames)
        {
            return _patchRequestProvider.GetPatchObjectWithFields(fieldNames);
        }
    }
}
