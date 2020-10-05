using Patcheetah.Attributes;
using System;

namespace Patcheetah.Tests.Models
{
    public class PersonalInfo
    {
        [RequiredOnPatching]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [IgnoreOnPatching]
        public DateTime Birthday { get; set; }
    }
}
