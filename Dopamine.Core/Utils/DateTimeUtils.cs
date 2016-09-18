using System;

namespace Dopamine.Core.Utils
{
    public static class DateTimeUtils
    {
        public static long ConvertToUnixTime(DateTime dateTime)
        {
            DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime.ToUniversalTime() - referenceTime).TotalSeconds;
        }
    }
}
