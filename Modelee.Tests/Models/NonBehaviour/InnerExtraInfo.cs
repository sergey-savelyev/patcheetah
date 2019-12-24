using Modelee.Attributes;

namespace Modelee.Tests.Models.NonBehaviour
{
    public class InnerExtraInfo : IInnerExtraInfo
    {
        public string InfoString { get; set; }

        public int InfoCounter { get; set; }
    }
}
