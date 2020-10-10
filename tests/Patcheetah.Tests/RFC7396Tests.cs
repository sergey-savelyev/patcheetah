using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Patcheetah.JsonNET;
using Patcheetah.Patching;
using Patcheetah.Tests.Models.RFC7396;
using System.Linq;

namespace Patcheetah.Tests
{
    public class RFC7396Tests
    {
        [Test]
        public void RFC7396PatchingEnabledTest()
        {
            PatchEngineCore.Cleanup();
            PatchEngine.Init(cfg =>
            {
                cfg.EnableNestedPatching();
            });

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
            patchObject.ApplyTo(model);

            Assert.AreEqual("+01-123-456-7890", model.PhoneNumber);
            Assert.IsNull(model.Author.FamilyName);
            Assert.AreEqual("John", model.Author.GivenName);
            Assert.AreEqual(1, model.Tags.Length);
            Assert.AreEqual("example", model.Tags.First());
        }

        [Test]
        public void RFC7396PatchingDisabledTest()
        {
            PatchEngineCore.Cleanup();
            PatchEngine.Init();

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
            patchObject.ApplyTo(model);

            Assert.AreEqual("+01-123-456-7890", model.PhoneNumber);
            Assert.IsNull(model.Author.FamilyName);
            Assert.IsNull(model.Author.GivenName);
            Assert.AreEqual(1, model.Tags.Length);
            Assert.AreEqual("example", model.Tags.First());
        }
    }
}
