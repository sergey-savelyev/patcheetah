using Patcheetah.Patching;
using Patcheetah.Tests.Models.Abstract;
using System.Text.Json;

namespace Patcheetah.Tests.Helpers
{
    public class SystemTextPatchRequestProvider<TUser> : IPatchRequestProvider<TUser>
        where TUser : class, IUser
    {
        public PatchObject<TUser> GetPatchObjectWithFields(params string[] fields)
        {
            var jobject = PatchRequestsConstructor.GetRequestWithFields(fields);
            var jsonRaw = jobject.ToString();
            var stjObject = JsonSerializer.Deserialize<PatchObject<TUser>>(jsonRaw);

            return stjObject;
        }
    }
}
