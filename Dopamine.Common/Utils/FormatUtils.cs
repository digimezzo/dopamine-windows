using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using System;
using System.Globalization;
using System.Text;

namespace Dopamine.Common.Utils
{
    public static class FormatUtils
    {
        public static string FormatDuration(long duration)
        {
            var sb = new StringBuilder();

            TimeSpan ts = TimeSpan.FromMilliseconds(duration);

            if (ts.Days > 0)
            {
                return string.Concat(string.Format("{0:n1}", ts.TotalDays), " ", ts.TotalDays < 1.1 ? ResourceUtils.GetStringResource("Language_Day") : ResourceUtils.GetStringResource("Language_Days"));
            }

            if (ts.Hours > 0)
            {
                return string.Concat(string.Format("{0:n1}", ts.TotalHours), " ", ts.TotalHours < 1.1 ? ResourceUtils.GetStringResource("Language_Hour") : ResourceUtils.GetStringResource("Language_Hours"));
            }

            if (ts.Minutes > 0)
            {
                sb.Append(string.Concat(ts.ToString("%m"), " ", ts.Minutes == 1 ? ResourceUtils.GetStringResource("Language_Minute") : ResourceUtils.GetStringResource("Language_Minutes"), " "));
            }

            if (ts.Seconds > 0)
            {
                sb.Append(string.Concat(ts.ToString("%s"), " ", ts.Seconds == 1 ? ResourceUtils.GetStringResource("Language_Second") : ResourceUtils.GetStringResource("Language_Seconds")));
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
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.GigaByteInBytes, ResourceUtils.GetStringResource("Language_Gigabytes_Short"));
            }
            else if (sizeInBytes >= Constants.MegaByteInBytes)
            {
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.MegaByteInBytes, ResourceUtils.GetStringResource("Language_Megabytes_Short"));
            }
            else if (sizeInBytes >= Constants.KiloByteInBytes)
            {
                humanReadableSize = string.Format("{0:#.#} {1}", sizeInBytes / Constants.KiloByteInBytes, ResourceUtils.GetStringResource("Language_Kilobytes_Short"));
            }

            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            if (showByteSize)
            {
                return string.Format("{0} ({1} {2})", humanReadableSize, sizeInBytes.ToString("#,#", nfi), ResourceUtils.GetStringResource("Language_Bytes").ToLower());
            }
            else
            {
                return string.Format("{0}", humanReadableSize);
            }
        }
    }
}
