using System;
namespace Modelee.Exceptions
{
    public class PropertyRequiredException : Exception
    {
        public PropertyRequiredException(string propertyName)
            : base($"Property {propertyName} is required")
        {
        }
    }
}
