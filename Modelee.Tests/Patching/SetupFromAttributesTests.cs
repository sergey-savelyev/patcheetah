using Modelee.Tests.Models.Behaviour;
using NUnit.Framework;

namespace Modelee.Tests.Patching
{
    public class SetupFromAttributesTests : EssentialTests<TestModelWithAttrs, ExtraInfoWithAttrs, InnerExtraInfoWithAttrs>
    {
        protected override void Setup()
        {
            // We don't need to setup, cause we use models setted up by attributes
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
            PassCaseSensitiveTest(true);
        }
    }
}
