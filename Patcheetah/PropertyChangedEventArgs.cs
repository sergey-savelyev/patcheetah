using System;
using System.Reflection;

namespace Patcheetah
{
    public class PropertyChangedEventArgs : EventArgs
    {
        public object SourceValue { get; set; }

        public object TargetValue { get; set; }

        public object Entity { get; set; }
    }
}