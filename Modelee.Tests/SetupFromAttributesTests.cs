using Modelee.Tests.Models.WithAttributes;
using NUnit.Framework;

namespace Modelee.Tests
{
    public class SetupFromAttributesTests : TestBase<User, PersonalInfo, Contact, UserAddress>
    {
        protected override void Setup()
        {
            // We don't need to setup, cause we use models setted up by attributes
        }

        [Test]
        public void CaseSensitivityTest()
        {
            CaseSensitiveTest(true);
        }
    }
}
