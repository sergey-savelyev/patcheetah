using System.Collections.Generic;
using Patcheetah.Attributes;
using Patcheetah.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Patcheetah.Tests.Models.WithAttributes
{
    [CaseSensitivePatching]
    public class User : IUser<PersonalInfo, Contact, UserAddress>
    {
        [PatchingKey]
        public string Id { get; set; }

        [IgnoreOnPatching]
        public string Username { get; set; }

        public string NickName { get; set; }

        [RequiredOnPatching]
        public string LastSeenFrom { get; set; }

        // [ConfiguredPatching]
        [JsonAlias("Personal")]
        public PersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public Contact[] ArchivedContacts { get; set; }

        // [ConfiguredPatching]
        public List<Contact> Contacts { get; set; }

        public UserRole Role { get; set; }
    }
}
