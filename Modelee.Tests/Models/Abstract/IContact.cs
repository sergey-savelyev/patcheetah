namespace Modelee.Tests.Models.Abstract
{
    public interface IContact<TUserAddress>
        where TUserAddress : IUserAddress
    {
        public string Id { get; set; }

        public ContactType Type { get; set; }

        public string Value { get; set; }

        public TUserAddress Address { get; set; }
    }
}
