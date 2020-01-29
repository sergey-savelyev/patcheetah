using System;
using Modelee.Attributes;
using Modelee.Tests.Models.Abstract;

namespace Modelee.Tests.Models.WithAttributes
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
