using System;
using NUnit.Framework;
using Modelee.Configuration;
using Modelee.Tests.Models.NonBehaviour;

namespace Modelee.Tests.Patching
{
    public class SetupFromConfigTests : EssentialTests<TestModel, ExtraInfo, InnerExtraInfo>
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
                .UseModeleeConfig(x => x.ExtraInfoList)
                .Register(x => x.Id);

            ModeleeConfig.CreateFor<ExtraInfo>()
                .IgnoreOnPatching(x => x.Description)
                .UseModeleeConfig(x => x.InnerExtraInfo) // create additional test where config for inner info has not been created
                .Register(x => x.Id);

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

        [Test]
        public void ArrayPatchingTest()
        {
            PassArrayPatchingTest();
        }

        [Test]
        public void WrongTypeTest()
        {
            PassWrongTypeTest();
        }

        [Test]
        public void NullPatchingTest()
        {
            PassNullPatchingTest();
        }

        [Test]
        public void CaseSensitivityTest()
        {
            PassCaseSensitiveTest(false);
        }
    }
}
