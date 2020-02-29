using System;
namespace Patcheetah.Mapping
{
    public static class Mapping
    {
        public static MappingResultTyped<T> CreateFor<T>(T value) => new MappingResultTyped<T>(value);
    }
}