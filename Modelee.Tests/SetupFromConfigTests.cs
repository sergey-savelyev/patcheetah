using NUnit.Framework;
using Modelee.Configuration;
using Modelee.Tests.Models.Standard;

namespace Modelee.Tests
{
    public class SetupFromConfigTests : TestBase<User, PersonalInfo, Contact, UserAddress>
    {
        protected override void Setup()
        {
            ModeleeConfig.CreateFor<User>()
                .Required(x => x.LastSeenFrom)
                .IgnoreOnPatching(x => x.Username)
                .UseModeleeConfig(x => x.PersonalInfo)
                .UseModeleeConfig(x => x.Contacts)
                .Register(x => x.Id);

            ModeleeConfig.CreateFor<PersonalInfo>()
                .IgnoreOnPatching(x => x.Birthday)
                .UseModeleeConfig(x => x.Address)
                .Register();

            ModeleeConfig.CreateFor<Contact>()
                .IgnoreOnPatching(x => x.Type)
                .UseModeleeConfig(x => x.Address)
                .Register(x => x.Id);

            ModeleeConfig.CreateFor<UserAddress>()
                .Required(x => x.FullAddress)
                .Register();
        }

        [Test]
        public void CaseSensitivityTest()
        {
            CaseSensitiveTest(false);
        }
    }
}
