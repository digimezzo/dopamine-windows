using System.Collections.Generic;

namespace Dopamine.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
                dict.Remove(key);
        }
    }
}