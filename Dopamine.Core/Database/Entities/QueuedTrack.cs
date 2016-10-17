using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class QueuedTrack
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long QueuedTrackID { get; set; }
        public string Path { get; set; }
        public long OrderID { get; set; }

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

            return this.PathToLower.Equals(((QueuedTrack)obj).PathToLower);
        }

        public override int GetHashCode()
        {
            return new { this.PathToLower }.GetHashCode();
        }
        #endregion
    }
}
