using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Patcheetah.Patching;
using Patcheetah.Tests.Models.RFC7396;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patcheetah.Tests
{
    public class RFC7396Tests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            PatchEngine.Reset();
            PatchEngine.Setup(cfg =>
            {
                cfg.EnableNestedPatching();
            });
        }

        [Test]
        public void RFC7396PatchingTest()
        {
            var model = new Post
            {
                Title = "Hello!",
                Author = new Author
                {
                    GivenName = "John",
                    FamilyName = "Doe"
                },
                Tags = new[] { "example", "sample" },
                Content = "This will be unchanged"
            };

            var jsonPatch = @"{
                                 ""title"": ""Hello!"",
                                 ""phoneNumber"": ""+01-123-456-7890"",
                                 ""author"": {
                                            ""familyName"": null
                                 },
                                 ""tags"": [ ""example"" ]
                               }";

            var patchObject = JObject.Parse(jsonPatch).ToObject<PatchObject<Post>>();
            patchObject.Patch(model);

            Assert.AreEqual("+01-123-456-7890", model.PhoneNumber);
            Assert.IsNull(model.Author.FamilyName);
            Assert.AreEqual(1, model.Tags.Length);
            Assert.AreEqual("example", model.Tags.First());
        }
    }
}
