using System;

namespace Modelee.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewModelNameAttribute : Attribute
    {
        public string Name { get; }

        public ViewModelNameAttribute(string name)
        {
            Name = name;
        }
    }
}
