using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Digimezzo.Utilities.Log;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.Utils
{
    public sealed class OpenFileUtils
    {
        public static async Task<bool> OpenImageFileAsync(Action<byte[]> callback)
        {
            bool isOpenSuccess = true;

            // Set up the file dialog box
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = ResourceUtils.GetStringResource("Language_Select_Image");
            dlg.DefaultExt = FileFormats.JPG; // Default file extension
            dlg.Filter = ResourceUtils.GetStringResource("Language_Images") + " (*" + FileFormats.PNG + ";*" + FileFormats.JPG + ";*" + FileFormats.JPEG + ";*" + FileFormats.BMP + ")|*" + FileFormats.PNG + ";*" + FileFormats.JPG + ";*" + FileFormats.JPEG + ";*" + FileFormats.BMP; // Filter files by extension

            // Show the file dialog box
            bool? dialogResult = dlg.ShowDialog();

            // Process the file dialog box result
            if (dialogResult.HasValue & dialogResult.Value)
            {
                byte[] byteArray = null;

                await Task.Run(() =>
                {
                    try
                    {
                        byteArray = ImageUtils.Image2ByteArray(dlg.FileName);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("An error occured while converting the image to a Byte[]. Exception: {0}", ex.Message);
                        isOpenSuccess = false;
                    }
                });

                if (byteArray != null)
                {
                    callback(byteArray);
                }
                else
                {
                    isOpenSuccess = false;
                }
            }

            return isOpenSuccess;
        }
    }
}
