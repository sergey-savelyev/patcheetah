using NUnit.Framework;
using Patcheetah.Configuration;
using Patcheetah.Tests.Models.Standard;

namespace Patcheetah.Tests
{
    public abstract class SetupFromConfigTests : TestBase<User>
    {
        protected void Configure(PatcheetahConfig config)
        {
            config
                .ConfigureEntity<User>()
                .Required(x => x.LastSeenFrom)
                .IgnoreOnPatching(x => x.Login)
                .UseJsonAlias(x => x.PersonalInfo, "Personal")
                .SetKey(x => x.Id);
        }

        [Test]
        public void CaseSensitivityTest()
        {
            CaseSensitiveTest(false);
        }

        [Test]
        public void KeyTest()
        {
            KeyTest("Id");
        }
    }
}
