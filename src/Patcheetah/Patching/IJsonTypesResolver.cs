using System;

namespace Patcheetah.Patching
{
    public interface IJsonTypesResolver
    {
        object ResolveType(object value, Type type);

        T ResolveJsonType<T>(object value);

        bool IsArray(object value);

        bool IsObject(object value);
    }
}
