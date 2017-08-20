using System;
using System.Windows.Data;
using System.Globalization;

namespace Dopamine.Common.Presentation.Converters
{
    public class StringToLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString();

            return !string.IsNullOrEmpty(str) ? str.ToLower() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
