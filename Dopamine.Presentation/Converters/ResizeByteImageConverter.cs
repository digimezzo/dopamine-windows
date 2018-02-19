using Digimezzo.Utilities.Utils;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Dopamine.Presentation.Converters
{
    public class ResizeByteImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                byte[] byteImage = values[0] as byte[];
                int size = Int32.Parse(values[1].ToString());

                if (byteImage != null)
                {
                    byte[] resizeByteImage = ImageUtils.ResizeImageInByteArray(byteImage, size, size);
                    byteImage = null;
                    return ImageUtils.ByteToBitmapImage(resizeByteImage, size, size, 0);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
