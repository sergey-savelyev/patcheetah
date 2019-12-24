using System;

namespace Modelee.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotIncludedInViewModelAttribute : IgnoreOnPatchingAttribute
    {
    }
}
