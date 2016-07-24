using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Converters
{
    public class StringEmptyToInvisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            return (string.IsNullOrEmpty(value.ToString()) ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
