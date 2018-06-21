using SQLite;

namespace Dopamine.Data.Entities
{
    public class TrackStatistic
    {
        [PrimaryKey(), AutoIncrement()]
        public long TrackStatisticID { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public long? Rating { get; set; }

        public long? Love { get; set; }

        public long? PlayCount { get; set; }

        public long? SkipCount { get; set; }

        public long? DateLastPlayed { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.SafePath.Equals(((TrackStatistic)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return new { this.SafePath }.GetHashCode();
        }
    }
}
