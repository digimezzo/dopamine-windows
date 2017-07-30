using System;
using Windows.UI.Xaml.Data;

namespace Dopamine.UWP.Converters
{
    public class StringToLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                string str = value.ToString();

                if (!string.IsNullOrEmpty(str))
                {
                    return str.ToLower();
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
