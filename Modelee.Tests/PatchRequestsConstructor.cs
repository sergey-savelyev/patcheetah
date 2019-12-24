using System;
using Modelee.Tests.Models.NonBehaviour;
using Newtonsoft.Json.Linq;

namespace Modelee.Tests
{
    public class PatchRequestsConstructor
    {
        private static readonly JToken _originalJToken;

        public static string GeneratedId { get; }

        public static PatchObject<TestModel> OriginalModel
            => _originalJToken.ToObject<PatchObject<TestModel>>();

        static PatchRequestsConstructor()
        {
            GeneratedId = Guid.NewGuid().ToString();

            _originalJToken = new JObject
            {
                { "Id", GeneratedId },
                { "Name", "Random name" },
                { "Description", "Random description" },
                { "Counter", 1 },
                { "AdditionalInfo", GetExtraInfo() },
                { "ExtraInfoArray", GetExtraInfos(3, "Random extra info array description") },
                { "ExtraInfoList", GetExtraInfos(3, "Random extra info list description") },
                { "IntegerArray", JToken.FromObject(new int[] { 1, 2, 3, 4, 5, 6 }) },
                { "OnlyModelString", "Random only model string" }
            };
        }

        public static JArray GetExtraInfos(int count, string description)
        {
            var array = new JArray();

            for (var i = 0; i < count; i++)
            {
                var item = GetExtraInfo($"{description} {i + 1}", i + 1);
                array.Add(item);
            }

            return array;
        }

        public static JObject GetExtraInfo(string description = null, int? innerCounter = null)
        {
            var result = new JObject
            {
                { "Id", Guid.NewGuid().ToString() },
                { "Description", description ?? "Random extra info description" },
                { "InnerExtraInfo", GetInnerExtraInfo(innerCounter ?? 1) }
            };

            return result;
        }

        public static JObject GetInnerExtraInfo(int counter, string infoString = null)
        {
            var result = new JObject
            {
                { "InfoCounter", counter },
                { "Info", infoString ?? $"Random inner info string {counter}" }
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
