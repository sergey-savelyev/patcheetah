using System;
using System.Collections.Generic;
using System.Reflection;

namespace Patcheetah.Configuration
{
    public interface ICustomAttributesConfigurator
    {
        void Configure(PropertyInfo property, IEnumerable<Attribute> propertyAttributes, EntityConfig config);
    }
}
