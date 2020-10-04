using System;
using System.Collections.Generic;
using Patcheetah.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Patcheetah.Tests.Models.Standard
{
    public class User : IUser
    {
        public string Id { get; set; }

        public long Age { get; set; }

        public string Username { get; set; }

        public string Login { get; set; }

        public string LastSeenFrom { get; set; }

        [JsonProperty("Personal")]
        public PersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public List<Contact> Contacts { get; set; }

        public Contact[] ArchivedContacts { get; set; }

        public UserRole Role { get; set; }
    }
}
