using NUnit.Framework;
using Patcheetah.Tests.Models.Standard;

namespace Patcheetah.Tests
{
    public class SetupFromConfigTests : TestBase<User, PersonalInfo, Contact, UserAddress>
    {
        protected override void Setup()
        {
            PatchConfig.CreateForEntity<User>()
                .Required(x => x.LastSeenFrom)
                .IgnoreOnPatching(x => x.Username)
                .UseJsonAlias(x => x.PersonalInfo, "Personal")
                // .UsePatcheetahConfig(x => x.PersonalInfo)
                // .UsePatcheetahConfig(x => x.Contacts)
                .Register(x => x.Id);

            PatchConfig.CreateForEntity<PersonalInfo>()
                .IgnoreOnPatching(x => x.Birthday)
                // .UsePatcheetahConfig(x => x.Address)
                .Register();

            PatchConfig.CreateForEntity<Contact>()
                .IgnoreOnPatching(x => x.Type)
                // .UsePatcheetahConfig(x => x.Address)
                .Register(x => x.Id);

            PatchConfig.CreateForEntity<UserAddress>()
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
