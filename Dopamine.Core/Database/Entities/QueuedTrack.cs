using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class QueuedTrack
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long QueuedTrackID { get; set; }
        public string Path { get; set; }
        public string SafePath { get; set; }
        public long OrderID { get; set; }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.SafePath.Equals(((QueuedTrack)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return new { this.SafePath }.GetHashCode();
        }
        #endregion
    }
}
