using SQLite;

namespace Dopamine.Data.Entities
{
    public class Blacklist
    {
        [PrimaryKey(), AutoIncrement()]
        public long BlacklistID { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public Blacklist()
        {
        }

        public Blacklist(long blacklistId, string artist, string title, string path, string safePath)
        {
            this.BlacklistID = blacklistId;
            this.Artist = Artist;
            this.Title = Title;
            this.Path = path;
            this.SafePath = safePath;
        }
    }
}
