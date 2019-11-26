using Modelee.Attributes;

namespace Modelee.Tests.Models.NonBehaviour
{
    public class ExtraInfo
    {
        public string Description { get; set; }

        public InnerExtraInfo InnerExtraInfo { get; set; }
    }
}
