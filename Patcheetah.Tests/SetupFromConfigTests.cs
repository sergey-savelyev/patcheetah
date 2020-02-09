using NUnit.Framework;
using Patcheetah.Configuration;
using Patcheetah.Tests.Models.Standard;

namespace Patcheetah.Tests
{
    public class SetupFromConfigTests : TestBase<User, PersonalInfo, Contact, UserAddress>
    {
        protected override void Setup()
        {
            PatcheetahConfig.CreateFor<User>()
                .Required(x => x.LastSeenFrom)
                .IgnoreOnPatching(x => x.Username)
                .UsePatcheetahConfig(x => x.PersonalInfo)
                .UsePatcheetahConfig(x => x.Contacts)
                .Register(x => x.Id);

            PatcheetahConfig.CreateFor<PersonalInfo>()
                .IgnoreOnPatching(x => x.Birthday)
                .UsePatcheetahConfig(x => x.Address)
                .Register();

            PatcheetahConfig.CreateFor<Contact>()
                .IgnoreOnPatching(x => x.Type)
                .UsePatcheetahConfig(x => x.Address)
                .Register(x => x.Id);

            PatcheetahConfig.CreateFor<UserAddress>()
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
