using System;
using System.Collections.Generic;

namespace Dopamine.Core.Extensions
{
    public static class ListExtensions
    {
        public static string FirstNonEmpty(this IEnumerable<string> strings, string alternateString)
        {
            foreach (string item in strings)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    return item;
                }
            }

            return alternateString;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static List<T> Randomize<T>(this List<T> list)
        {
            var randomList = new List<T>();

            var r = new Random();

            int randomIndex = 0;

            while (list.Count > 0)
            {
                randomIndex = r.Next(0, list.Count);  // Choose a random object in the list
                randomList.Add(list[randomIndex]); // Add it to the new, random list
                list.RemoveAt(randomIndex); // Remove to avoid duplicates
            }

            return randomList;
        }

    }
}
