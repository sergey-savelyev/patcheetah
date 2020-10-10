using System;

namespace Patcheetah.Exceptions
{
    public class TypeMissmatchException : Exception
    {
        public string JsonType { get; }

        public string PropertyType { get; }

        public string PropertyName { get; }

        public TypeMissmatchException(string newTypeName, string oldTypeName, string propertyName, Exception inner)
            : base($"Type missmatch detected. Property: {propertyName}. Property type: {oldTypeName}. Incoming type: {newTypeName}.", inner)
        {
            JsonType = newTypeName;
            PropertyType = oldTypeName;
            PropertyName = propertyName;
        }
    }
}
