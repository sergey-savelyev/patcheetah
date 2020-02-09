using System;
namespace Patcheetah.Mapping
{
    public class MappingResult
    {
        public object Value { get; }

        internal bool Skip { get; set; }

        internal MappingResult(Type type, object value)
        {
            if (value.GetType() != type)
            {
                this.Skip = true;

                return;
            }

            Value = value;
        }
    }
}
