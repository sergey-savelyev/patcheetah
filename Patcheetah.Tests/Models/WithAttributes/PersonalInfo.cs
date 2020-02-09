using System;
using System.Collections.Generic;
using Patcheetah.Attributes;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Models.WithAttributes
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
