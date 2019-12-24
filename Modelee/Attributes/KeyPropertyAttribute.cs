using System;

namespace Modelee.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyPropertyAttribute : Attribute
    {
        public bool Strict { get; }

        public KeyPropertyAttribute(bool strict = false)
        {
            Strict = strict;
        }
    }
}
