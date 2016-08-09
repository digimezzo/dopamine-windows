using System;

namespace Dopamine.Core.Utils
{
    public static class ArrayUtils
    {
        public static double[] ConvertArray(string[] array)
        {
            return Array.ConvertAll(array, s => double.Parse(s));
        }
    }
}
