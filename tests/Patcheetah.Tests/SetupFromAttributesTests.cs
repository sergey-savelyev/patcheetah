using Patcheetah.Tests.Models.WithAttributes;
using NUnit.Framework;

namespace Patcheetah.Tests
{
    public class SetupFromAttributesTests : TestBase<User>
    {
        protected override void Setup()
        {
            // We don't need to setup, cause we use models setted up by attributes
            // All that we need is just to enable it
            PatchEngine.Setup(cfg =>
            {
                cfg.EnableAttributes();
            });
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
