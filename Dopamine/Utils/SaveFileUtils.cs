using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;

namespace Dopamine.Utils
{
    public class SaveFileUtils
    {
        public static async Task<bool> SaveImageFileAsync(string filename, byte[] data)
        {
            return await SaveImageFileAsync(filename, data, 0, 0, 100L);
        }

        public static async Task<bool> SaveImageFileAsync(string filename, byte[] data, int width, int height, long qualityPercent)
        {
            bool isSaveSuccess = true;

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp",
                AddExtension = true,
                FileName = filename,
                OverwritePrompt = true
            };

            bool? dlgResult = dlg.ShowDialog();
            if (dlgResult.HasValue & dlgResult.Value)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string mime = string.Empty;
                        switch (dlg.SafeFileName.Split('.')[dlg.SafeFileName.Split('.').Length - 1].ToLower())
                        {
                            case "png":
                                mime = "image/png";
                                break;
                            case "jpg":
                                mime = "image/jpeg";
                                break;
                            case "bmp":
                                mime = "image/bmp";
                                break;
                        }
                        var codec = ImageCodecInfo.GetImageEncoders().First(encoder => encoder.MimeType == mime);

                        ImageUtils.Byte2ImageFile(data, codec, dlg.FileName, width, height, qualityPercent);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("An error occurred while saving the byte[] to a file. Exception: {0}", ex.Message);
                        isSaveSuccess = false;
                    }
                });
            }

            return isSaveSuccess;
        }
    }
}