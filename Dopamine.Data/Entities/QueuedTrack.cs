using SQLite;

namespace Dopamine.Data.Entities
{
    public class QueuedTrack
    {
        [PrimaryKey()]
        public long TrackID { get; set; }

        public long IsPlaying { get; set; }

        public long ProgressSeconds { get; set; }

        public long OrderID { get; set; }
      
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return TrackID == ((QueuedTrack)obj).TrackID;
        }

        public override int GetHashCode()
        {
            return TrackID.GetHashCode();
        }
    }
}
