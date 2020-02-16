using System;
using System.Collections.Generic;
using System.Reflection;

namespace Patcheetah.Configuration
{
    public interface IConfigurationBehaviour
    {
        string Id { get; }

        void Configure(PropertyInfo property, IEnumerable<Attribute> attributes, EntityConfig config);
    }
}
