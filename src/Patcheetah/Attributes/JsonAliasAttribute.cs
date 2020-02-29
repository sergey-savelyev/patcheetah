using System;

namespace Patcheetah.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonAliasAttribute : Attribute
    {
        public string Alias { get; set; }

        public JsonAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
