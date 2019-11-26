using System;
using System.Collections.Generic;
using System.Linq;

namespace Modelee.Collections
{
    public class CaseSensitiveList : List<string>
    {
        private bool _caseSensitive;

        public CaseSensitiveList(bool caseSensitive)
        {
            _caseSensitive = caseSensitive;
        }

        public void SetCaseSensitivity(bool caseSensitive)
        {
            _caseSensitive = caseSensitive;
        }

        public bool Contains(string element)
        {
            var index = this.FindIndex(x => x.Equals(element, _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));

            return index != -1;
        }

        public IEnumerable<string> Except(IEnumerable<string> collection)
        {
            var comparer = _caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            return this.Except(collection, comparer);
        }
    }
}
