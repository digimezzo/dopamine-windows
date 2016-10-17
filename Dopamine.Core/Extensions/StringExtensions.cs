using System;

namespace Dopamine.Core.Extensions
{
    public static class StringExtensions
    {
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
            return path.ToLower();
        }
    }
}
