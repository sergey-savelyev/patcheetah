using System;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Models.Standard
{
    public class Contact : IContact<UserAddress>
    {
        public string Id { get; set; }

        public ContactType Type { get; set; }

        public string Value { get; set; }

        public UserAddress Address { get; set; }
    }
}
