using System;
namespace Patcheetah.Exceptions
{
    public class MultipleKeyException : Exception
    {
        public MultipleKeyException()
            : base("Multiple keys not supported")
        {
        }
    }
}
