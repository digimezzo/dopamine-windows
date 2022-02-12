using SQLite;

namespace Dopamine.Data.Entities
{
    public class BlacklistTrack
    {
        [PrimaryKey(), AutoIncrement()]
        public long BlacklistTrackID { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public BlacklistTrack()
        {
        }

        public BlacklistTrack(long blacklistTrackId, string path, string safePath)
        {
            this.BlacklistTrackID = blacklistTrackId;
            this.Path = path;
            this.SafePath = safePath;
        }
    }
}
