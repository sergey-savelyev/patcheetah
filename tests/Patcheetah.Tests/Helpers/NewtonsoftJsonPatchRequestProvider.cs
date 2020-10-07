using Patcheetah.Patching;
using Patcheetah.Tests.Models.Abstract;

namespace Patcheetah.Tests.Helpers
{
    public class NewtonsoftJsonPatchRequestProvider<TUser> : IPatchRequestProvider<TUser>
        where TUser: class, IUser
    {
        public PatchObject<TUser> GetPatchObjectWithFields(params string[] fields)
        {
            return PatchRequestsConstructor.GetRequestWithFields(fields).ToObject<PatchObject<TUser>>();
        }
    }
}
