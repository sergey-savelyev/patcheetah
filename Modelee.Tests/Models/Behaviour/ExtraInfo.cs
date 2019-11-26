using Modelee.Attributes;

namespace Modelee.Tests.Models.Pack1
{
    [CaseSensitivePatching]
    public class ExtraInfo
    {
        [IgnoreOnPatching]
        public string Description { get; set; }

        [UseModelee]
        public InnerExtraInfo InnerExtraInfo { get; set; }
    }
}
