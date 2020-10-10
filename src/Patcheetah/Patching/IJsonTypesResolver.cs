using System;
using System.Reflection;

namespace Patcheetah.Patching
{
    public interface IJsonTypesResolver
    {
        object ResolveType(object value, Type type);

        T ResolveType<T>(object value);

        bool IsArray(object value);

        bool IsObject(object value);
    }
}
