using System;
using Patcheetah.Mapping;

namespace Patcheetah.Configuration
{
    public class PropertyConfiguration
    {
        private string _name;

        public bool Key { get; set; }

        public string Name => Alias ?? _name;

        public bool Required { get; set; }

        public bool Ignored { get; set; }

        public bool HasInternalConfig { get; set; }

        public string Alias { get; set; }

        public MappingHandler MappingHandler { get; set; }

        public Action<PropertyChangedEventArgs> BeforePatchCallback { get; set; }

        public Action<PropertyChangedEventArgs> AfterPatchCallback { get; set; }

        public PropertyConfiguration(string name)
        {
            _name = name;
        }
    }
}
