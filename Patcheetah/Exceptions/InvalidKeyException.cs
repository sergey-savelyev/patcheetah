using System;
namespace Patcheetah.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException(string message)
            : base(message)
        {
        }

        public InvalidKeyException(Type keyType)
            : base($"Key with type {keyType.Name} does not implement IComparable")
        {
        }
    }
}
