using System;

namespace Modelee.Configuration
{
    public class PropertyConfiguration
    {
        private string _name;

        public bool Key { get; set; }

        public string Name => ViewModelAlias ?? _name;

        public bool Required { get; set; }

        public bool Ignored { get; set; }

        public bool NotIncludedInViewModel { get; set; }

        public bool HasModeleeCongig { get; set; }

        public string ViewModelAlias { get; set; }

        public Action<PropertyChangedEventArgs> BeforePatchCallback { get; set; }

        public Action<PropertyChangedEventArgs> AfterPatchCallback { get; set; }

        public PropertyConfiguration(string name)
        {
            _name = name;
        }
    }
}
