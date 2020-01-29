using System;
using System.Collections.Generic;
using System.Linq;
using Modelee.Exceptions;
using Modelee.Tests.Models;
using Modelee.Tests.Models.Abstract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Modelee.Tests
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
        public void JsonPropertyAttributeTest()
        {
            var request = GetPatchRequestWithFields("Id", "LastSeenFrom", "Personal");
            var innerAddressString = request["Personal"]["Address"]["Address"].ToString();
            var model = request.CreateEntity();

            Assert.NotNull(innerAddressString);
            Assert.AreEqual(innerAddressString, model.PersonalInfo.Address.FullAddress);
        }

        [Test]
        public void RecursiveModeleePatchingTest()
        {
            var model = new TUser
            {
                PersonalInfo = new TPersonalInfo
                {
                    Birthday = new DateTime(1985, 01, 01)
                }
            };
            var birthdayFromModel = model.PersonalInfo.Birthday.ToString();
            var request = GetPatchRequestWithFields("Id", "LastSeenFrom", "Personal");

            request.Patch(model);

            Assert.AreEqual(birthdayFromModel, model.PersonalInfo.Birthday.ToString());

            var brokenRequestJObject = JToken.FromObject(request).DeepClone();
            (brokenRequestJObject["Personal"]["Address"] as JObject).Remove("Address"); // remove required field
            var brokenRequest = brokenRequestJObject.ToObject<PatchObject<TUser>>();

            var ex = Assert.Throws<RequiredPropertiesMissedException>(() => brokenRequest.CreateEntity());
            Assert.AreEqual("Address", ex.Properties.First());
        }

        [Test]
        public void ArrayPatchingTest()
        {
            var request = GetPatchRequestWithFields("LastSeenFrom", "AccessRights", "ArchivedContacts", "Contacts");
            var accessRightsArrayFromRequestCount = request["AccessRights"].ToObject<string[]>().Length;
            var archivedContactsFromRequestCount = request["ArchivedContacts"].ToObject<TContact[]>().Length;

            var contactWithSameKey = request["Contacts"].ToObject<List<TContact>>().First();
            var requestContactType = contactWithSameKey.Type;

            var enumValues = Enum.GetValues(typeof(ContactType));
            foreach (var enumVal in enumValues)
            {
                if ((ContactType)enumVal != requestContactType)
                {
                    contactWithSameKey.Type = (ContactType)enumVal;
                }
            }
            
            contactWithSameKey.Address.FullAddress = "New full address";

            var model = new TUser
            {
                AccessRights = new[] { "Read", "Write", "Approve" },
                ArchivedContacts = new[]
                {
                    new TContact
                    {
                        Type = Models.ContactType.Email,
                        Id = Guid.NewGuid().ToString(),
                        Address = null
                    }
                },
                Contacts = new List<TContact>
                {
                    new TContact
                    {
                        Id = null,
                        Type = Models.ContactType.PhoneNumber
                    },
                    contactWithSameKey
                }
            };

            request.Patch(model);

            Assert.AreEqual(model.AccessRights.Length, accessRightsArrayFromRequestCount);
            Assert.AreEqual(model.ArchivedContacts.Length, archivedContactsFromRequestCount);
            Assert.AreEqual(model.Contacts.Count, 4); // change 4 to computed var
            Assert.AreEqual(model.Contacts.FirstOrDefault(x => x.Id == null)?.Type, Models.ContactType.PhoneNumber);
            Assert.AreNotEqual(model.Contacts.FirstOrDefault(x => x.Id == contactWithSameKey.Id)?.Type, requestContactType);
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

            var exception = Assert.Throws<JsonReaderException>(() =>
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
