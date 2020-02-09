using System;
using Patcheetah.Attributes;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Models.WithAttributes
{
    public class Contact : IContact<UserAddress>
    {
        [PatchingKey]
        public string Id { get; set; }

        [IgnoreOnPatching]
        public ContactType Type { get; set; }

        public string Value { get; set; }

        [ConfiguredPatching]
        public UserAddress Address { get; set; }
    }
}
