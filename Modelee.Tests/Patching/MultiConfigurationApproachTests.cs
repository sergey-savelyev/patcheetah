using System.Linq;
using Modelee.Configuration;
using Modelee.Exceptions;
using Modelee.Tests.Models.Behaviour;
using NUnit.Framework;

namespace Modelee.Tests.Patching
{
    public class MultiConfigurationApproachTests : EssentialTests<TestModelWithAttrs, ExtraInfoWithAttrs, InnerExtraInfoWithAttrs>
    {
        protected override void Setup()
        {
            ModeleeConfig.CreateFor<TestModelWithAttrs>()
                // .Required(x => x.Name) -> instead of method setup, we'll install it from attribute
                // .IgnoreOnPatching(x => x.Counter) -> Same situation, let's install it from attribute
                .AliasInViewModel(x => x.ExtraInfo, "ThisNameWillBeReplacedFromAttr")
                .Register(x => x.Name); // Set name as key but replace it by id from attributes
        }

        [Test]
        public void OverridingTest()
        {
            // required prop overriding

            var request = GetPatchRequestWithFields("Id");
            var model = new TestModelWithAttrs();
            var requiredException = Assert.Throws<RequiredPropertiesMissedException>(() =>
            {
                var newModel = request.CreateEntity();
            });

            Assert.AreEqual("Name", requiredException.Properties.First());

            // ignore prop overriding

            request = GetPatchRequestWithFields("Name", "Counter");
            var requestCounter = request["Counter"].ToObject<int>();
            var modelCounter = 5;
            model.Counter = modelCounter;

            Assert.AreNotEqual(modelCounter, requestCounter);

            model = request.Patch(model);

            Assert.AreEqual(modelCounter, model.Counter);

            // alias prop overriding

            request = GetPatchRequestWithFields("Name", "AdditionalInfo");
            var modelToCheck = request.CreateEntity();

            Assert.IsNotNull(modelToCheck.ExtraInfo);

            // key overriding test

            PassKeyTest();
        }
    }
}
