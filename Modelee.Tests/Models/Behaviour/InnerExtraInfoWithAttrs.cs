using Modelee.Attributes;

namespace Modelee.Tests.Models.Behaviour
{
    public class InnerExtraInfoWithAttrs : IInnerExtraInfo
    {
        [RequiredField]
        [ViewModelName("Info")]
        public string InfoString { get; set; }

        [NotIncludedInViewModel]
        public int InfoCounter { get; set; }
    }
}
