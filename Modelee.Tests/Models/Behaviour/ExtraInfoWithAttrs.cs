using Modelee.Attributes;

namespace Modelee.Tests.Models.Behaviour
{
    [CaseSensitivePatching]
    public class ExtraInfoWithAttrs : IExtraInfo<InnerExtraInfoWithAttrs>
    {
        [KeyProperty]
        public string Id { get; set; }

        [IgnoreOnPatching]
        public string Description { get; set; }

        [UseModelee]
        public InnerExtraInfoWithAttrs InnerExtraInfo { get; set; }
    }
}
