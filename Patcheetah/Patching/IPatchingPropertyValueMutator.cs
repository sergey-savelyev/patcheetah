using System.Reflection;
using Patcheetah.Configuration;

namespace Patcheetah.Patching
{
    public interface IPatchingPropertyValueMutator
    {
        string Id { get; }

        object MutateNewValueForProperty(
            object newValueBeforeMutation,
            PropertyInfo propertyInfo,
            PropertyConfiguration propertyConfiguration,
            object currentPropertyValue);
    }
}
