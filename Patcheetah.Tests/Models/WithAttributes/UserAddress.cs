using Patcheetah.Attributes;
using Patcheetah.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Patcheetah.Tests.Models.WithAttributes
{
    public class UserAddress : IUserAddress
    {
        [JsonProperty("Address")]
        [RequiredOnPatching]
        public string FullAddress { get; set; }

        public long Zip { get; set; }
    }
}
