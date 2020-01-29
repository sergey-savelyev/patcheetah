using Modelee.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Modelee.Tests.Models.Standard
{
    public class UserAddress : IUserAddress
    {
        [JsonProperty("Address")]
        public string FullAddress { get; set; }

        public long Zip { get; set; }
    }
}
