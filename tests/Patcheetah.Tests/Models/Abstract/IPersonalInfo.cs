using System;
namespace Patcheetah.Tests.Models.Abstract
{
    public interface IPersonalInfo<TUserAddress>
        where TUserAddress : IUserAddress
    {
        public Gender Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        public TUserAddress Address { get; set; }
    }
}
