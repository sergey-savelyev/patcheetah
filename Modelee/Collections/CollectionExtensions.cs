using System;
using System.Collections.Generic;

namespace Modelee.Collections
{
    public static class CollectionExtensions
    {
        public static void AddIfNotExist<T>(this ICollection<T> list, T element)
            where T : IComparable
        {
            if (!list.Contains(element))
            {
                list.Add(element);
            }
        }

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