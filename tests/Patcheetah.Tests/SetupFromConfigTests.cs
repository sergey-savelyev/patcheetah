using NUnit.Framework;
using Patcheetah.Tests.Models.Standard;

namespace Patcheetah.Tests
{
    public class SetupFromConfigTests : TestBase<User>
    {
        protected override void Setup()
        {
            PatchEngine.Setup(cfg =>
            {
                cfg
                    .ConfigureEntity<User>()
                    .Required(x => x.LastSeenFrom)
                    .IgnoreOnPatching(x => x.Login)
                    .UseJsonAlias(x => x.PersonalInfo, "Personal")
                    .SetKey(x => x.Id);
            });
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
