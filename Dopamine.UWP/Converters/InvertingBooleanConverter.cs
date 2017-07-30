using System;
using Windows.UI.Xaml.Data;

namespace Dopamine.UWP.Converters
{
    public class InvertingBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!object.ReferenceEquals(targetType, typeof(bool)))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
