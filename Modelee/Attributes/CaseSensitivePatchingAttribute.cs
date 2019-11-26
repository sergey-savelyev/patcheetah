using System;

namespace Modelee.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CaseSensitivePatchingAttribute : Attribute
    {
        public bool CaseSensitive { get; }

        public CaseSensitivePatchingAttribute(bool caseSensitive = true)
        {
            CaseSensitive = caseSensitive;
        }
    }
}
