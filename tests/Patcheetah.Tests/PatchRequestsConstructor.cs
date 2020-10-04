using System;
using Patcheetah.Tests.Models;
using Patcheetah.Tests.Models.Standard;
using Newtonsoft.Json.Linq;
using Patcheetah.Patching;

namespace Patcheetah.Tests
{
    public class PatchRequestsConstructor
    {
        private static readonly JToken _originalJToken;

        public static string GeneratedId { get; }

        public static PatchObject<User> OriginalModel
            => _originalJToken.ToObject<PatchObject<User>>();

        static PatchRequestsConstructor()
        {
            GeneratedId = Guid.NewGuid().ToString();

            _originalJToken = new JObject
            {
                { "Id", GeneratedId },
                { "Age", new JValue((int)26) },
                { "Username", "Patcherman" },
                { "Login", "Patcherman" },
                { "LastSeenFrom", "Nokia 3310" },
                { "Personal", GetPersonalInfo("Sergey", "Savelyev", new DateTime(1994, 4, 19)) },
                { "AccessRights", JToken.FromObject(new string[] { "FullAccess" }) },
                { "Contacts", GetContacts(3, "ContactValue") },
                { "Role", UserRole.Admin.ToString() },
            };
        }

        public static JObject GetPersonalInfo(string firstName, string lastName, DateTime birthday)
        {
            var result = new JObject
            {
                { "FirstName", firstName },
                { "LastName", lastName },
                { "Birthday", birthday },
            };

            return result;
        }

        public static JArray GetContacts(int count, string valuePattern)
        {
            var array = new JArray();

            for (var i = 1; i <= count; i++)
            {
                var item = GetContact(i, valuePattern);
                array.Add(item);
            }

            return array;
        }

        public static JObject GetContact(int counter, string valuePattern)
        {
            var result = new JObject
            {
                { "Type", $"type_{counter}" },
                { "Value", $"{valuePattern}_{counter}" },
            };

            return result;
        }

        public static JToken GetRequestWithFields(params string[] keys)
        {
            var result = new JObject();

            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (key.Split('.').Length > 1)
                {
                    continue;
                }

                var value = _originalJToken[key];

                result.Add(key, value);
            }

            return result;
        }
    }
}
