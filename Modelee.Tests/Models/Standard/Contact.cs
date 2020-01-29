using System;
using Modelee.Tests.Models.Abstract;

namespace Modelee.Tests.Models.Standard
{
    public class Contact : IContact<UserAddress>
    {
        public string Id { get; set; }

        public ContactType Type { get; set; }

        public string Value { get; set; }

        public UserAddress Address { get; set; }
    }
}
