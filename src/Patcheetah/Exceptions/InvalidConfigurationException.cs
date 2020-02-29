using System;
namespace Patcheetah.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        public string[] Properties { get; }

        public InvalidConfigurationException(string message)
            : base(message)
        {
        }

        public InvalidConfigurationException(string message, params string[] propertyNames)
            : base(message)
        {
            Properties = propertyNames;
        }
    }
}
