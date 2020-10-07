using Patcheetah.Tests.Models.WithAttributes;
using NUnit.Framework;
using Patcheetah.Configuration;

namespace Patcheetah.Tests
{
    public abstract class SetupFromAttributesTests : TestBase<User>
    {
        protected void Configure(PatcheetahConfig config)
        {
            config.EnableAttributes();
        }

        [Test]
        public void CaseSensitivityTest()
        {
            CaseSensitiveTest(true);
        }

        [Test]
        public void KeyTest()
        {
            KeyTest("Id");
        }
    }
}
