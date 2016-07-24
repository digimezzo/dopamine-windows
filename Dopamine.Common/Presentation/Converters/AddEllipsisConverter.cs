using System;
using System.Globalization;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Converters
{
    public class AddEllipsisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (!object.ReferenceEquals(value.GetType(), typeof(string))))
            {
                return Binding.DoNothing;
            }

            return string.Concat(value.ToString(), "...");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
