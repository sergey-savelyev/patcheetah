using System;
using System.Collections.Generic;
using Modelee.Attributes;
using Modelee.Tests.Models.Abstract;

namespace Modelee.Tests.Models.WithAttributes
{
    public class PersonalInfo : IPersonalInfo<UserAddress>
    {
        public Gender Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [IgnoreOnPatching]
        public DateTime Birthday { get; set; }

        [ConfiguredPatching]
        public UserAddress Address { get; set; }
    }
}
