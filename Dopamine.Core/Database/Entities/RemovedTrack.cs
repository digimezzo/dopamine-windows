using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class RemovedTrack
    {
        #region Properties
        [PrimaryKey()]
        public long TrackID { get; set; }
        public string Path { get; set; }
        public long DateRemoved { get; set; }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Path.Equals(((RemovedTrack)obj).Path);
        }

        public override int GetHashCode()
        {
            return new { this.Path }.GetHashCode();
        }
        #endregion
    }
}
