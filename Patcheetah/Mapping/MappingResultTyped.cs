using System;
namespace Patcheetah.Mapping
{
    public class MappingResultTyped<TProperty> : MappingResult
    {
        internal MappingResultTyped(TProperty value)
            : base(typeof(TProperty), value)
        {
        }

        public MappingResultTyped<TProperty> Skip()
        {
            base.Skip = true;

            return this;
        }
    }
}
