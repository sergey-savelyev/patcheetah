using System;
using System.Linq;
using Modelee.Exceptions;
using Modelee.Tests.Models.NonBehaviour;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Modelee.Tests.Patching
{
    public abstract class EssentialTests<TModel>
        where TModel : class, new()
    {
        [OneTimeSetUp]
        public void SetupInternal()
        {
            Setup();
        }

        protected abstract void Setup();

        public void PassKeyTest()
        {
            var requestWithKey = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id", "Name");
            Assert.IsFalse(requestWithKey.IsNew);

            var requestWithoutKey = PatchRequestsGenerator.GetRequestWithFields<TModel>("Name");
            Assert.IsTrue(requestWithoutKey.IsNew);
        }

        public void PassRequiredTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var request = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id", "Name");
                var entity = request.CreateEntity();
            });

            var exception = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var request = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id");
                var entity = request.CreateEntity();
            });

            Assert.That(exception.Properties.First(), Is.EqualTo("Name"));
        }

        public void PassIgnoredTest()
        {
            var counterValue = 8;

            var model = new TestModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Testee",
                Counter = counterValue
            };

            var request = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id", "Name", "Counter");

            model = request.Patch(model as TModel) as TestModel;

            Assert.AreEqual(model.Counter, counterValue);
        }

        public void PassViewModelNameTest()
        {
            var request = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id", "Name", "AdditionalInfo");
            var extraInfoInnerInfoString = request["AdditionalInfo"]["InnerExtraInfo"]["Info"].ToString();
            var model = request.CreateEntity();

            Assert.NotNull(extraInfoInnerInfoString);
            Assert.AreEqual(extraInfoInnerInfoString, (model as TestModel).ExtraInfo.InnerExtraInfo.InfoString);
        }

        public void PassRecursiveModeleePatchingTest()
        {
            var description = "Weeeee testeee!";
            var request = PatchRequestsGenerator.GetRequestWithFields<TModel>("Id", "Name", "AdditionalInfo");
            var createdExtraInfoDescription = request["AdditionalInfo"]["Description"].ToString();

            var model = new TModel();
            (model as TestModel).ExtraInfo = new ExtraInfo
            {
                Description = description
            };
            model = request.Patch(model);

            Assert.AreEqual(description, (model as TestModel).ExtraInfo.Description);

            var brokenRequestJObject = JToken.FromObject(request).DeepClone();
            (brokenRequestJObject["AdditionalInfo"]["InnerExtraInfo"] as JObject).Remove("Info"); // removing required field
            var brokenRequest = brokenRequestJObject.ToObject<PatchObject<TModel>>();

            var ex = Assert.Throws<RequiredPropertiesMissedException>(() => brokenRequest.CreateEntity());
            Assert.AreEqual("Info", ex.Properties.First());
        }
    }
}
