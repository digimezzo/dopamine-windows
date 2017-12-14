using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using System;
using System.Globalization;
using System.Text;

namespace Dopamine.Core.Utils
{
    public static class FormatUtils
    {
        public static string FormatDuration(long duration)
        {
            var sb = new StringBuilder();

            TimeSpan ts = TimeSpan.FromMilliseconds(duration);

            if (ts.Days > 0)
            {
                return string.Concat(string.Format("{0:n1}", ts.TotalDays), " ", ts.TotalDays < 1.1 ? ResourceUtils.GetString("Language_Day") : ResourceUtils.GetString("Language_Days"));
            }

            if (ts.Hours > 0)
            {
                return string.Concat(string.Format("{0:n1}", ts.TotalHours), " ", ts.TotalHours < 1.1 ? ResourceUtils.GetString("Language_Hour") : ResourceUtils.GetString("Language_Hours"));
            }

            if (ts.Minutes > 0)
            {
                sb.Append(string.Concat(ts.ToString("%m"), " ", ts.Minutes == 1 ? ResourceUtils.GetString("Language_Minute") : ResourceUtils.GetString("Language_Minutes"), " "));
            }

            if (ts.Seconds > 0)
            {
                sb.Append(string.Concat(ts.ToString("%s"), " ", ts.Seconds == 1 ? ResourceUtils.GetString("Language_Second") : ResourceUtils.GetString("Language_Seconds")));
            }

            return sb.ToString();
        }

        public static string FormatTime(TimeSpan ts)
        {
            if (ts.Hours > 0)
            {
                return ts.ToString("hh\\:mm\\:ss");
            }
            else
            {
                return ts.ToString("m\\:ss");
            }
        }

        public static string FormatFileSize(long sizeInBytes, bool showByteSize = true)
        {

            string humanReadableSize = string.Empty;

            if (sizeInBytes >= Constants.GigaByteInBytes)
            {
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.GigaByteInBytes, ResourceUtils.GetString("Language_Gigabytes_Short"));
            }
            else if (sizeInBytes >= Constants.MegaByteInBytes)
            {
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.MegaByteInBytes, ResourceUtils.GetString("Language_Megabytes_Short"));
            }
            else if (sizeInBytes >= Constants.KiloByteInBytes)
            {
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.KiloByteInBytes, ResourceUtils.GetString("Language_Kilobytes_Short"));
            }

            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            if (showByteSize)
            {
                return string.Format("{0} ({1} {2})", humanReadableSize, sizeInBytes.ToString("#,#", nfi), ResourceUtils.GetString("Language_Bytes").ToLower());
            }
            else
            {
                return string.Format("{0}", humanReadableSize);
            }
        }

        public static bool ParseLyricsTime(string input, out TimeSpan result)
        {
            try
            {
                int minutes = Convert.ToInt32(input.Split(':')[0]);
                string secondsAndMilliseconds = input.Split(':')[1];
                int seconds = Convert.ToInt32(secondsAndMilliseconds.Split('.')[0]);
                int milliseconds = Convert.ToInt32(secondsAndMilliseconds.Split('.')[1]);

                result = TimeSpan.FromMilliseconds(minutes * 60000 + seconds * 1000 + milliseconds);
                return true;
            }
            catch (Exception)
            {
            }

            result = new TimeSpan();
            return false;
        }
    }
}
