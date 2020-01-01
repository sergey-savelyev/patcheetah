using System;
using System.Collections.Generic;
using System.Linq;
using Modelee.Exceptions;
using Modelee.Tests.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Modelee.Tests.Patching
{
    public abstract class EssentialTests<TTestModel, TExtraInfo, TInnerExtraInfo>
        where TTestModel : class, ITestModel<TExtraInfo, TInnerExtraInfo>, new()
        where TExtraInfo : class, IExtraInfo<TInnerExtraInfo>, new()
        where TInnerExtraInfo : class, IInnerExtraInfo, new()
    {
        [OneTimeSetUp]
        public void SetupInternal()
        {
            Setup();
        }

        protected abstract void Setup();

        public void PassPartialPatchingTest()
        {
            var request = GetPatchRequestWithFields("Id");
        }

        public void PassKeyTest()
        {
            var requestWithKey = GetPatchRequestWithFields("Id", "Name");
            Assert.IsFalse(requestWithKey.HasKey);

            var requestWithoutKey = GetPatchRequestWithFields("Name");
            Assert.IsTrue(requestWithoutKey.HasKey);
        }

        public void PassRequiredTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var request = GetPatchRequestWithFields("Id", "Name");
                var entity = request.CreateEntity();
            });

            var exception = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var request = GetPatchRequestWithFields("Id");
                var entity = request.CreateEntity();
            });

            Assert.That(exception.Properties.First(), Is.EqualTo("Name"));
        }

        public void PassIgnoredTest()
        {
            var counterValue = 8;

            var model = new TTestModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Testee",
                Counter = counterValue
            };

            var request = GetPatchRequestWithFields("Id", "Name", "Counter");

            model = request.Patch(model);

            Assert.AreEqual(model.Counter, counterValue);
        }

        public void PassViewModelNameTest()
        {
            var request = GetPatchRequestWithFields("Id", "Name", "AdditionalInfo");
            var extraInfoInnerInfoString = request["AdditionalInfo"]["InnerExtraInfo"]["Info"].ToString();
            var model = request.CreateEntity();

            Assert.NotNull(extraInfoInnerInfoString);
            Assert.AreEqual(extraInfoInnerInfoString, model.ExtraInfo.InnerExtraInfo.InfoString);
        }

        public void PassRecursiveModeleePatchingTest()
        {
            var description = "Weeeee testeee!";
            var request = GetPatchRequestWithFields("Id", "Name", "AdditionalInfo");
            var createdExtraInfoDescription = request["AdditionalInfo"]["Description"].ToString();

            var model = new TTestModel
            {
                ExtraInfo = new TExtraInfo
                {
                    Description = description
                }
            };

            model = request.Patch(model);

            Assert.AreEqual(description, model.ExtraInfo.Description);

            var brokenRequestJObject = JToken.FromObject(request).DeepClone();
            (brokenRequestJObject["AdditionalInfo"]["InnerExtraInfo"] as JObject).Remove("Info"); // remove required field
            var brokenRequest = brokenRequestJObject.ToObject<PatchObject<TTestModel>>();

            var ex = Assert.Throws<RequiredPropertiesMissedException>(() => brokenRequest.CreateEntity());
            Assert.AreEqual("Info", ex.Properties.First());
        }

        public void PassArrayPatchingTest()
        {
            var request = GetPatchRequestWithFields("Name", "IntegerArray", "ExtraInfoArray", "ExtraInfoList");
            var intArrayFromRequestCount = request["IntegerArray"].ToObject<int[]>().Length;
            var extraInfoArrayFromRequestCount = request["ExtraInfoArray"].ToObject<TExtraInfo[]>().Length;

            var extraInfoWithSameKey = request["ExtraInfoList"].ToObject<List<TExtraInfo>>().First();
            var requestDescription = extraInfoWithSameKey.Description;
            extraInfoWithSameKey.Description = "Weeeee testeee!";
            extraInfoWithSameKey.InnerExtraInfo.InfoString = "So good and testee";

            var model = new TTestModel
            {
                IntegerArray = new[] { 1, 2, 3 },
                ExtraInfoArray = new[]
                {
                    new TExtraInfo
                    {
                        Description = "Some description",
                        Id = Guid.NewGuid().ToString(),
                        InnerExtraInfo = null
                    }
                },
                ExtraInfoList = new List<TExtraInfo>
                {
                    new TExtraInfo
                    {
                        Id = null,
                        Description = "Existing extra info that should not be patched"
                    },
                    extraInfoWithSameKey
                }
            };

            model = request.Patch(model);

            Assert.AreEqual(model.IntegerArray.Length, intArrayFromRequestCount);
            Assert.AreEqual(model.ExtraInfoArray.Length, extraInfoArrayFromRequestCount);
            Assert.AreEqual(model.ExtraInfoList.Count, 4); // change 4 to computed var
            Assert.AreEqual(model.ExtraInfoList.FirstOrDefault(x => x.Id == null)?.Description, "Existing extra info that should not be patched");
            Assert.AreNotEqual(model.ExtraInfoList.FirstOrDefault(x => x.Id == extraInfoWithSameKey.Id)?.Description, requestDescription);
        }

        public void PassWrongTypeTest()
        {
            var request = GetPatchRequestWithFields("Name", "IntegerArray");
            request["IntegerArray"] = new JArray
            {
                "str1",
                "str2"
            };

            var model = new TTestModel
            {
                IntegerArray = new[] { 1, 2, 3 }
            };

            var exception = Assert.Throws<JsonReaderException>(() =>
            {
                model = request.Patch(model);
            });
        }

        public void PassNullPatchingTest()
        {
            // property with modelee config
            var request = GetPatchRequestWithFields("Name");
            request.Add("AdditionalInfo", null);
            var model = new TTestModel
            {
                Description = "Test",
                ExtraInfo = new TExtraInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = "Test",
                    InnerExtraInfo = null
                },
                ExtraInfoList = new List<TExtraInfo>
                {
                    new TExtraInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Description = "Test",
                        InnerExtraInfo = null
                    }
                },
                ExtraInfoArray = new TExtraInfo[]
                {
                    new TExtraInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Description = "Test",
                        InnerExtraInfo = null
                    }
                }
            };

            model = request.Patch(model);

            Assert.AreEqual(null, model.ExtraInfo);

            // property with no modelee config
            request = GetPatchRequestWithFields("Name");
            request.Add("Description", null);
            model = request.Patch(model);

            Assert.AreEqual(null, model.Description);

            // array with modelee config
            request = GetPatchRequestWithFields("Name");
            request.Add("ExtraInfoList", null);
            model = request.Patch(model);

            Assert.AreEqual(null, model.ExtraInfoList);

            // array with no modelee config
            request = GetPatchRequestWithFields("Name");
            request.Add("ExtraInfoArray", null);
            model = request.Patch(model);

            Assert.AreEqual(null, model.ExtraInfoArray);
        }

        public void PassCaseSensitiveTest(bool sensitive)
        {
            var request = GetPatchRequestWithFields("Name", "Description");
            var descriptionValue = request["Description"].ToString();
            request.Remove("Description", out _);
            request.Add("description", descriptionValue);

            var modelDescription = "RandomDescription";
            var model = new TTestModel
            {
                Description = modelDescription
            };

            model = request.Patch(model);

            if (sensitive)
            {
                Assert.AreNotEqual(descriptionValue, model.Description);

                return;
            }

            Assert.AreEqual(descriptionValue, model.Description);
        }

        protected PatchObject<TTestModel> GetPatchRequestWithFields(params string[] fieldNames)
        {
            return PatchRequestsConstructor.GetRequestWithFields(fieldNames).ToObject<PatchObject<TTestModel>>();
        }
    }
}
