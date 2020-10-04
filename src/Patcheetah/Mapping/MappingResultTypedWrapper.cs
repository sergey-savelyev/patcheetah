using System;
namespace Patcheetah.Mapping
{
    public class MappingResultTypedWrapper<TProperty> : MappingResult
    {
        internal MappingResultTypedWrapper(TProperty value)
            : base(typeof(TProperty), value)
        {
        }
    }
}
