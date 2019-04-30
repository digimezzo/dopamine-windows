namespace Dopamine.Core.Extensions
{
    public static class LongExtensions
    {
        public static bool HasValueLargerThan(this long sourceLong, long value)
        {
            if (sourceLong > value)
            {
                return true;
            }

            return false;
        }

        public static bool HasValueLargerThan(this long? sourceLong, long value)
        {
            if (sourceLong.HasValue && sourceLong.Value > value)
            {
                return true;
            }

            return false;
        }

        public static long GetValueOrZero(this long? sourceLong)
        {
            if (sourceLong.HasValue)
            {
                return sourceLong.Value;
            }

            return 0;
        }
    }
}
