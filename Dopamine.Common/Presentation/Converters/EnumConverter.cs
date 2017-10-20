using System;
using System.Globalization;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int returnValue = 0;

            if (parameter is Type)
            {
                returnValue = (int)Enum.Parse((Type)parameter, value.ToString());
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
