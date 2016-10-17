using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class RemovedTrack
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long TrackID { get; set; }
        public string Path { get; set; }
        public long DateRemoved { get; set; }

        [Ignore()]
        public string PathToLower
        {
            get
            {
                return this.Path.ToLower();
            }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.PathToLower.Equals(((RemovedTrack)obj).PathToLower);
        }

        public override int GetHashCode()
        {
            return new { this.PathToLower }.GetHashCode();
        }
        #endregion
    }
}
