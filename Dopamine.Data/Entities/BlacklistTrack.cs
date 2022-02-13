using SQLite;

namespace Dopamine.Data.Entities
{
    public class BlacklistTrack
    {
        [PrimaryKey(), AutoIncrement()]
        public long BlacklistTrackID { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public BlacklistTrack()
        {
        }

        public BlacklistTrack(long blacklistTrackId, string artist, string title, string path, string safePath)
        {
            this.BlacklistTrackID = blacklistTrackId;
            this.Artist = artist;
            this.Title = title;
            this.Path = path;
            this.SafePath = safePath;
        }
    }
}
