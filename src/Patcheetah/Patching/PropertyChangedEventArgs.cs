using Patcheetah.Configuration;
using System;

namespace Patcheetah.Patching
{
    public class PropertyChangedEventArgs : EventArgs
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public object Entity { get; set; }

        public PropertyConfiguration PropertyConfiguration { get; set; }
    }
}