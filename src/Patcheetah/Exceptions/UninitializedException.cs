using System;

namespace Patcheetah.Exceptions
{
    public class UninitializedException : Exception
    {
        public UninitializedException()
            : base("Patch engine was not initialized")
        {
        }
    }
}
