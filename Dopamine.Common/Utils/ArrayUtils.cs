using System.Collections.Generic;
using System.Globalization;

namespace Dopamine.Common.Utils
{
    public static class ArrayUtils
    {
        public static double[] ConvertArray(string[] array)
        {
            var values = new List<double>();

            for (int i = 0; i < array.Length; i++)
            {
                double doubleValue = default(double);
                double.TryParse(array[i],NumberStyles.Number, CultureInfo.InvariantCulture, out doubleValue);
                values.Add(doubleValue);
            }
            
            return values.ToArray();
        }
    }
}
