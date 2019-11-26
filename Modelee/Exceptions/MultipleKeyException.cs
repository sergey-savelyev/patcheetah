using System;
namespace Modelee.Exceptions
{
    public class MultipleKeyException : Exception
    {
        public MultipleKeyException()
            : base("Multiple keys not supported")
        {
        }
    }
}
