namespace Patcheetah.Tests.Models.Abstract
{
    public interface IUserAddress
    {
        public string FullAddress { get; set; }

        public long Zip { get; set; }
    }
}
