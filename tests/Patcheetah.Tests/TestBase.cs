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
    public abstract class TestBase<TUser, TPersonalInfo, TContact, TUserAddress>
        where TUser : class, IUser<TPersonalInfo, TContact, TUserAddress>, new()
        where TPersonalInfo : class, IPersonalInfo<TUserAddress>, new()
        where TContact : class, IContact<TUserAddress>, new()
        where TUserAddress : class, IUserAddress, new()
    {
        [OneTimeSetUp]
        public void SetupInternal()
        {
            Setup();
        }

        [Test]
        public void KeyTest()
        {
            var requestWithKey = GetPatchRequestWithFields("Id", "LastSeenFrom");
            Assert.IsFalse(requestWithKey.HasKey);

            var requestWithoutKey = GetPatchRequestWithFields("LastSeenFrom");
            Assert.IsTrue(requestWithoutKey.HasKey);
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
                Username = "Testee",
            };

            var usernameValue = model.Username;
            var request = GetPatchRequestWithFields("Id", "Username", "LastSeenFrom");

            request.Patch(model);

            Assert.AreEqual(model.Username, usernameValue);
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
                NickName = "Testee",
                PersonalInfo = new TPersonalInfo
                {
                    Gender = Gender.Unknown,
                    Address = null,
                    Birthday = new DateTime(1987, 02, 02),
                    FirstName = "Random",
                    LastName = "Person"
                },
                Contacts = new List<TContact>
                {
                    new TContact
                    {
                        Id = Guid.NewGuid().ToString(),
                        Address = null,
                        Type = ContactType.Email,
                        Value = "random@mail.test"
                    }
                },
                ArchivedContacts = new TContact[]
                {
                    new TContact
                    {
                        Id = Guid.NewGuid().ToString(),
                        Address = null,
                        Type = ContactType.Email,
                        Value = "random@mail.test"
                    }
                }
            };

            request.Patch(model);

            Assert.AreEqual(null, model.PersonalInfo);

            // property with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("NickName", null);
            request.Patch(model);

            Assert.AreEqual(null, model.NickName);

            // array with modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("Contacts", null);
            request.Patch(model);

            Assert.AreEqual(null, model.Contacts);

            // array with no modelee config
            request = GetPatchRequestWithFields("LastSeenFrom");
            request.Add("ArchivedContacts", null);
            request.Patch(model);

            Assert.AreEqual(null, model.ArchivedContacts);
        }

        protected void CaseSensitiveTest(bool sensitive)
        {
            var request = GetPatchRequestWithFields("LastSeenFrom", "NickName");
            var nicknameValue = request["NickName"].ToString();
            request.Remove("NickName", out _);
            request.Add("nickname", nicknameValue);

            var modelNickname = "Testee";
            var model = new TUser
            {
                NickName = modelNickname
            };

            request.Patch(model);

            if (sensitive)
            {
                Assert.AreNotEqual(nicknameValue, model.NickName);

                return;
            }

            Assert.AreEqual(nicknameValue, model.NickName);
        }

        protected abstract void Setup();

        protected PatchObject<TUser> GetPatchRequestWithFields(params string[] fieldNames)
        {
            return PatchRequestsConstructor.GetRequestWithFields(fieldNames).ToObject<PatchObject<TUser>>();
        }
    }
}
