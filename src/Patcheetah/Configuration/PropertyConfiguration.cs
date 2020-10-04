using System;
using System.Collections.Generic;
using Patcheetah.Mapping;
using Patcheetah.Patching;

namespace Patcheetah.Configuration
{
    public class PropertyConfiguration
    {
        public bool IsKey { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public bool Ignored { get; set; }

        public MappingHandler MappingHandler { get; set; }

        public Func<PropertyChangedEventArgs, object> BeforeMappingCallback { get; set; }

        public Action<PropertyChangedEventArgs> AfterSetCallback { get; set; }

        public Dictionary<string, object> ExtraSettings { get; set; } = new Dictionary<string, object>();

        public PropertyConfiguration(string name)
        {
            Name = name;
        }
    }
}
