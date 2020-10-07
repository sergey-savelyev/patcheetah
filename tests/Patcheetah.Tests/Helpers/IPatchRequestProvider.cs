using Patcheetah.Patching;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Helpers
{
    public interface IPatchRequestProvider<TUser>
        where TUser: class, IUser
    {
        PatchObject<TUser> GetPatchObjectWithFields(params string[] fields);
    }
}
