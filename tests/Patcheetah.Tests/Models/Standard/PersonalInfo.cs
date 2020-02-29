using System;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Models.Standard
{
    public class PersonalInfo : IPersonalInfo<UserAddress>
    {
        public Gender Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        public UserAddress Address { get; set; }
    }
}
