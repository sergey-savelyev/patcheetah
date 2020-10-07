using System.Collections.Generic;

namespace Patcheetah.Tests.Models.Abstract
{
    public interface IUser
    {
        public string Id { get; set; }

        public int Age { get; set; }

        public string Username { get; set; }

        public string Login { get; set; }

        public string LastSeenFrom { get; set; }

        public PersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public List<Contact> Contacts { get; set; }

        public UserRole Role { get; set; }
    }
}
