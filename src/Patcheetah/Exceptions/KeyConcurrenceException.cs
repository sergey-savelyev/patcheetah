using System;
namespace Patcheetah.Exceptions
{
    public class KeyConcurrenceException : Exception
    {
        public KeyConcurrenceException(object key)
            : base($"Patching objects with the same keys is not possible. Key: {key.ToString()}")
        {
        }
    }
}
