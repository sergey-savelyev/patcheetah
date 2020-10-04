using System;
using System.Collections.Generic;

namespace Patcheetah
{
    public static class Extensions
    {
        public static void RewriteIfExist<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
            where TKey : IComparable
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }

            dictionary.Add(key, value);
        }
    }
}