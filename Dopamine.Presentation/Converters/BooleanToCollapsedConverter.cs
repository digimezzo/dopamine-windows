using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Presentation.Converters
{
    public class BooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = bool.Parse(value.ToString());
            return vis ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            return (vis == Visibility.Visible);
        }
    }

    public class InvertingBooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = bool.Parse(value.ToString());
            return vis ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            return (vis == Visibility.Collapsed);
        }
    }
}
