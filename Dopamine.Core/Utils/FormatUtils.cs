using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
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
                var split = input.Split(':');
                if (split.Length == 0)
                {
                    result = new TimeSpan();
                    return false;
                }
                int minutes = Convert.ToInt32(split[0]);
                string secondsAndMilliseconds = split[1];

                split = secondsAndMilliseconds.Split('.');
                int seconds = Convert.ToInt32(split[0]);
                int milliseconds = split.Length == 1 ? 0 : Convert.ToInt32(split[1]);

                result = TimeSpan.FromMilliseconds(minutes * 60000 + seconds * 1000 + milliseconds);
                return true;
            }
            catch (Exception)
            {
            }

            result = new TimeSpan();
            return false;
        }

        public static string GetSortableString(string originalString, bool removePrefix = false)
        {
            if (string.IsNullOrEmpty(originalString)) return string.Empty;

            string returnString = originalString.ToLower().Trim();

            if (removePrefix)
            {
                try
                {
                    returnString = returnString.TrimStart("the ").Trim();
                }
                catch (Exception)
                {
                    // Swallow
                }
            }

            return returnString;
        }

        public static string TrimValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Trim();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string DelimitValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return $"{Constants.ColumnValueDelimiter}{value.Trim()}{Constants.ColumnValueDelimiter}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
