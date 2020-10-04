using System.Collections.Generic;
using Patcheetah.Attributes;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Models.WithAttributes
{
    [CaseSensitivePatching]
    public class User : IUser
    {
        [PatchingKey]
        public string Id { get; set; }

        public long Age { get; set; }

        public string Username { get; set; }

        [IgnoreOnPatching]
        public string Login { get; set; }

        [RequiredOnPatching]
        public string LastSeenFrom { get; set; }

        [JsonAlias("Personal")]
        public PersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public List<Contact> Contacts { get; set; }

        public UserRole Role { get; set; }
    }
}
