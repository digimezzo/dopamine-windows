using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class PlaylistEntry
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long EntryID { get; set; }
        public long PlaylistID { get; set; }
        public long TrackID { get; set; }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.PlaylistID == ((PlaylistEntry)obj).PlaylistID & this.TrackID == ((PlaylistEntry)obj).TrackID;
        }

        public override int GetHashCode()
        {
            return new { this.PlaylistID , this.TrackID }.GetHashCode();
        }
        #endregion
    }
}
