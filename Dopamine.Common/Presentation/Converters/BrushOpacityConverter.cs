using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dopamine.Common.Presentation.Converters
{
    public class BrushOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SolidColorBrush) || parameter == null)
            {   
                return Binding.DoNothing;
            }

            SolidColorBrush opacityBrush = new SolidColorBrush(((SolidColorBrush)value).Color);
            double opacity = 1.0;

            if (double.TryParse(parameter.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,  out opacity))
            {
                opacityBrush.Opacity = opacity;
            }

            return opacityBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
