using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
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

    public class InvertingBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = bool.Parse(value.ToString());
            return vis ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            return (vis == Visibility.Hidden);
        }
    }
}
