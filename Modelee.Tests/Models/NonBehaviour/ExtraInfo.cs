using Modelee.Attributes;

namespace Modelee.Tests.Models.NonBehaviour
{
    public class ExtraInfo : IExtraInfo<InnerExtraInfo>
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public InnerExtraInfo InnerExtraInfo { get; set; }
    }
}
