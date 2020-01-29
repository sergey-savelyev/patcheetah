using Modelee.Attributes;
using Modelee.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Modelee.Tests.Models.WithAttributes
{
    public class UserAddress : IUserAddress
    {
        [JsonProperty("Address")]
        [RequiredOnPatching]
        public string FullAddress { get; set; }

        public long Zip { get; set; }
    }
}
