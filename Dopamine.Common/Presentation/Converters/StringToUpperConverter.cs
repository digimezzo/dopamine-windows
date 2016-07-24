using System;
using System.Globalization;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string str = value.ToString();

                if (!string.IsNullOrEmpty(str))
                {
                    // HACK: CultureInfo("en-US") provides better ToUpper for some Greek Characters
                    return str.ToUpper(new CultureInfo("en-US"));
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
