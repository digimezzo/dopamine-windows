using System;

namespace Dopamine.Core.Utils
{
    public static class StringUtils
    {
        public static string[] Split(this string source, string separator)
        {
            return source.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static string Trim(this string source, string stringToRemove)
        {
            return source.Trim(stringToRemove.ToCharArray());
        }
    }
}
