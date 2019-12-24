using System;
namespace Modelee.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException(Type keyType)
            : base($"Key with type {keyType.Name} does not implement IComparable")
        {
        }
    }
}
