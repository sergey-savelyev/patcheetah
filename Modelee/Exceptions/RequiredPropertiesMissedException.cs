using System;
using System.Collections.Generic;

namespace Modelee.Exceptions
{
    public class RequiredPropertiesMissedException : Exception
    {
        public ICollection<string> Properties { get; }

        public RequiredPropertiesMissedException(ICollection<string> properties)
            : base ("Required properties missed")
        {
            Properties = properties;
        }
    }

    //public class RequiredPropertyMissedException : Exception
    //{
    //    public RequiredPropertyMissedException(string propertyName)
    //        : base($"Property {propertyName} is required")
    //    {
    //    }
    //}
}
