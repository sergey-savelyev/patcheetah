using System;
using System.Collections.Generic;
using Modelee.Tests.Models.Abstract;
using Newtonsoft.Json;

namespace Modelee.Tests.Models.Standard
{
    public class User : IUser<PersonalInfo, Contact, UserAddress>
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string NickName { get; set; }

        public string LastSeenFrom { get; set; }

        [JsonProperty("Personal")]
        public PersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public List<Contact> Contacts { get; set; }

        public Contact[] ArchivedContacts { get; set; }

        public UserRole Role { get; set; }
    }
}
