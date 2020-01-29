using System;

namespace Modelee.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PatchingKeyAttribute : Attribute
    {
        public bool Strict { get; }

        public PatchingKeyAttribute(bool strict = false)
        {
            Strict = strict;
        }
    }
}
