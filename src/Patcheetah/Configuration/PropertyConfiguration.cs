using System;
using System.Collections.Generic;
using Patcheetah.Mapping;

namespace Patcheetah.Configuration
{
    public class PropertyConfiguration
    {
        public bool IsKey { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public bool Ignored { get; set; }

        public Func<object, MappingResult> MappingHandler { get; set; }

        public Dictionary<string, object> ExtraSettings { get; set; } = new Dictionary<string, object>();

        public PropertyConfiguration(string name)
        {
            Name = name;
        }
    }
}
