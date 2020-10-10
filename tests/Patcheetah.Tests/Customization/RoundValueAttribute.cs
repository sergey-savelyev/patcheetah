using System;

namespace Patcheetah.Tests.Customization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoundValueAttribute : Attribute
    {
        public const string PARAMETER_NAME = "RoundValue";
        public int Precision { get; }

        public RoundValueAttribute(int precision)
        {
            Precision = precision;
        }
    }
}
