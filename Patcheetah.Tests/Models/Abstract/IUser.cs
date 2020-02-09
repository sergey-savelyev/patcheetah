using System;
using System.Collections.Generic;

namespace Patcheetah.Tests.Models.Abstract
{
    public interface IUser<TPersonalInfo, TContact, TUserAddress>
        where TPersonalInfo : IPersonalInfo<TUserAddress>
        where TContact : IContact<TUserAddress>
        where TUserAddress : IUserAddress
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string NickName { get; set; }

        public string LastSeenFrom { get; set; }

        public TPersonalInfo PersonalInfo { get; set; }

        public string[] AccessRights { get; set; }

        public List<TContact> Contacts { get; set; }

        public TContact[] ArchivedContacts { get; set; }

        public UserRole Role { get; set; }
    }
}
