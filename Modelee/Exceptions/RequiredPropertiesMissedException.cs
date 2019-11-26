using System;
using System.Collections.Generic;

namespace Modelee.Exceptions
{
    public class RequiredPropertiesMissedException : Exception
    {
        public IEnumerable<string> Properties { get; }

        public RequiredPropertiesMissedException(IEnumerable<string> properties)
            : base ("Required properties missed")
        {
            Properties = properties;
        }
    }
}
