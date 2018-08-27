using System;
using System.Collections.Generic;

namespace Dopamine.Core.Extensions
{
    public static class StringExtensions
    {
        public static string[] Split(this string source, string separator)
        {
            return source.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static string Trim(this string source, string stringToRemove)
        {
            return source.Trim(stringToRemove.ToCharArray());
        }

        public static string TrimStart(this string input, string prefixToRemove)
        {
            if (input != null && prefixToRemove != null && input.StartsWith(prefixToRemove))
            {
                return input.Substring(prefixToRemove.Length, input.Length - prefixToRemove.Length);
            }
            else
            {
                return input;
            }
        }

        public static long SafeConvertToLong(this string str)
        {
            long parsedLong = 0;
            Int64.TryParse(str, out parsedLong);
            return parsedLong;
        }

        public static bool IsNumeric(this string str)
        {
            float output;
            return float.TryParse(str, out output);
        }

        public static string ToSafePath(this string path)
        {
            return path != null ? path.ToLower() : path;
        }

        public static string MakeUnique(this string proposedString, IList<string> existingStrings)
        {
            string uniqueString = proposedString;

            int number = 1;

            while (existingStrings.Contains(uniqueString))
            {
                number++;
                uniqueString = proposedString + " (" + number + ")";
            }

            return uniqueString;
        }
    }
}
