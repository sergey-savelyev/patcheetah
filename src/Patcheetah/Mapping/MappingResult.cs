using System;

namespace Patcheetah.Mapping
{
    public class MappingResult
    {
        internal object Value { get; }

        internal bool Skipped { get; private set; }

        internal MappingResult(Type type, object value)
        {
            if (value.GetType().GetHashCode() != type.GetHashCode())
            {
                Skipped = true;

                return;
            }

            Value = value;
        }

        public static MappingResultTypedWrapper<T> Skip<T>(T val)
        {
            var skipped = new MappingResultTypedWrapper<T>(val);
            skipped.Skipped = true;

            return skipped;
        }

        public static MappingResultTypedWrapper<T> MapTo<T>(T val)
        {
            return new MappingResultTypedWrapper<T>(val);
        }
    }
}
