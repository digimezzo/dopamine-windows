using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Dopamine.Core.IO
{
    public sealed class ImageOperations
    {
        public static byte[] Image2GrayScaleByteArray(string filename)
        {
            byte[] byteArray = null;

            try
            {
                if (string.IsNullOrEmpty(filename)) return null;

                Bitmap bmp = default(Bitmap);

                using (Bitmap tempBmp = new Bitmap(filename))
                {

                    bmp = MakeGrayscale(new Bitmap(tempBmp));
                }

                ImageConverter converter = new ImageConverter();

                byteArray = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
            }
            catch (Exception)
            {
                throw;
            }

            return byteArray;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            // Create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            // Get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            // Create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {new float[] {0.3f,0.3f,0.3f,0,0},
                                                                     new float[] {0.59f,0.59f,0.59f,0,0},
                                                                     new float[] {0.11f,0.11f,0.11f,0,0},
                                                                     new float[] {0,0,0,1,0},
                                                                     new float[] {0,0,0,0,1}
                                                                    });

            // Create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            // Set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            // Draw the original image on the new image using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            // Dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static byte[] Image2ByteArray(string filename)
        {
            byte[] byteArray = null;

            try
            {
                if (string.IsNullOrEmpty(filename))
                    return null;

                Bitmap bmp = default(Bitmap);

                using (Bitmap tempBmp = new Bitmap(filename))
                {
                    bmp = new Bitmap(tempBmp);
                }

                ImageConverter converter = new ImageConverter();

                byteArray = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
            }
            catch (Exception)
            {
                throw;
            }

            return byteArray;
        }

        public static void Byte2Jpg(byte[] imageData, string filename, int width, int height, long qualityPercent)
        {
            try
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegCodec = null;

                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.MimeType == "image/jpeg")
                    {
                        jpegCodec = codec;
                    }
                }

                using (System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imageData)))
                {

                    EncoderParameters encoderParams = new EncoderParameters();
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityPercent);

                    if (width > 0 & height > 0)
                    {
                        // Resize only if a iThumbnailWidth and iThumbnailHeight are set
                        using (System.Drawing.Image thumb = img.GetThumbnailImage(width, height, null, IntPtr.Zero))
                        {
                            try
                            {
                                thumb.Save(filename, jpegCodec, encoderParams);
                            }
                            catch (Exception)
                            {
                                // When saving fails, a corrupt file is left behind. Let's try to delete it.
                                if (System.IO.File.Exists(filename))
                                {
                                    System.IO.File.Delete(filename);
                                }

                                throw;
                            }
                        }
                    }
                    else
                    {
                        // Else, just save the original size to file
                        try
                        {
                            img.Save(filename, jpegCodec, encoderParams);
                        }
                        catch (Exception)
                        {
                            // When saving fails, a corrupt file is left behind. Let's try to delete it.
                            if (System.IO.File.Exists(filename))
                            {
                                System.IO.File.Delete(filename);
                            }

                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static long GetImageDataSize(byte[] imageData)
        {
            int size = 0;

            try
            {
                using (System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imageData)))
                {
                    size = img.Width * img.Height;
                }

            }
            catch (Exception)
            {
            }

            return size;
        }

        public static BitmapImage PathToBitmapImage(string path, int imageWidth, int imageHeight)
        {
            if (System.IO.File.Exists(path))
            {
                BitmapImage bi = new BitmapImage();

                bi.BeginInit();

                if (imageWidth > 0 && imageHeight > 0)
                {
                    bi.DecodePixelWidth = imageWidth;
                    bi.DecodePixelHeight = imageHeight;
                }

                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(path);
                bi.EndInit();
                bi.Freeze();

                return bi;
            }

            return null;
        }

        public static BitmapImage ByteToBitmapImage(byte[] byteData, int imageWidth, int imageHeight, int maxLength)
        {
            if (byteData != null && byteData.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(byteData))
                {

                    BitmapImage bi = new BitmapImage();

                    bi.BeginInit();

                    if (imageWidth > 0 && imageHeight > 0)
                    {
                        var size = new Size(imageWidth, imageHeight);
                        if (maxLength > 0) size = GetScaledSize(new Size(imageWidth, imageHeight), maxLength);

                        bi.DecodePixelWidth = size.Width;
                        bi.DecodePixelHeight = size.Height;
                    }

                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();

                    return bi;
                }
            }

            return null;
        }

        public static Size GetScaledSize(Size originalSize, int maxLength)
        {
            var scaledSize = new Size();

            if (originalSize.Height > originalSize.Width)
            {
                scaledSize.Height = maxLength;
                scaledSize.Width = Convert.ToInt32(((double)originalSize.Width / maxLength) * 100);
            }
            else
            {
                scaledSize.Width = maxLength;
                scaledSize.Height = Convert.ToInt32(((double)originalSize.Height / maxLength) * 100);
            }

            return scaledSize;
        }
    }
}
