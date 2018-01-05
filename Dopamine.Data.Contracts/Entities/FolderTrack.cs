using SQLite;

namespace Dopamine.Data.Contracts.Entities
{
    public class FolderTrack
    {
        [PrimaryKey(), AutoIncrement()]
        public long FolderTrackID { get; set; }
        public long FolderID { get; set; }
        public long TrackID { get; set; }

        public FolderTrack()
        {
        }

        public FolderTrack(long folderId, long trackId)
        {
            this.FolderID = folderId;
            this.TrackID = trackId;
        }
    }
}
