using Patcheetah.Configuration;

namespace Patcheetah.Patching
{
    public class PatchContext
    {
        public object NewValue { get; set; }

        public object OldValue { get; set; }

        public object Entity { get; set; }

        public PropertyConfiguration PropertyConfiguration { get; set; }
    }
}
