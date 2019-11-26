using Modelee.Attributes;

namespace Modelee.Tests.Models.Pack1
{
    public class InnerExtraInfo
    {
        [RequiredField]
        [ViewModelName("Info")]
        public string InfoString { get; set; }

        [NotIncludedInViewModel]
        public int InfoCounter { get; set; }
    }
}
