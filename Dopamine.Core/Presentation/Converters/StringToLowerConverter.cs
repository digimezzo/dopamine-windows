using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using System.Windows.Data;
using System.Globalization;
#endif

namespace Dopamine.Core.Presentation.Converters
{
    public class StringToLowerConverter : IValueConverter
    {
#if WINDOWS_UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            var str = value?.ToString();

            return !string.IsNullOrEmpty(str) ? str.ToLower() : value;
        }
#if WINDOWS_UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            throw new NotSupportedException();
        }
    }
}
