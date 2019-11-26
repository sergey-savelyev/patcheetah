using System;
using NUnit.Framework;
using Modelee.Configuration;
using Modelee.Tests.Models.NonBehaviour;

namespace Modelee.Tests.Patching
{
    public class SetupFromConfigTests : EssentialTests<TestModel>
    {
        protected override void Setup()
        {
            ModeleeConfig.CreateFor<TestModel>()
                .Required(x => x.Name)
                .IgnoreOnPatching(x => x.Counter)
                .AliasInViewModel(x => x.ExtraInfo, "AdditionalInfo")
                .NotIncludedInViewModel(x => x.OnlyModelString)
                .UseModeleeConfig(x => x.OnlyModelString)
                .UseModeleeConfig(x => x.ExtraInfo)
                .Register(x => x.Id);

            ModeleeConfig.CreateFor<ExtraInfo>()
                .IgnoreOnPatching(x => x.Description)
                .UseModeleeConfig(x => x.InnerExtraInfo) // create additional test where config for inner info has not been created
                .Register();

            ModeleeConfig.CreateFor<InnerExtraInfo>()
                .Required(x => x.InfoString)
                .AliasInViewModel(x => x.InfoString, "Info")
                .NotIncludedInViewModel(x => x.InfoCounter)
                .Register();
        }

        [Test]
        public void KeyTest()
        {
            PassKeyTest();
        }

        [Test]
        public void RequiredTest()
        {
            PassRequiredTest();
        }

        [Test]
        public void IgnoredTest()
        {
            PassIgnoredTest();
        }

        [Test]
        public void ViewModelNameTest()
        {
            PassViewModelNameTest();
        }

        [Test]
        public void RecursiveModeleePatchingTest()
        {
            PassRecursiveModeleePatchingTest();
        }
    }
}
