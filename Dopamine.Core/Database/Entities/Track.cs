using SQLite;

namespace Dopamine.Core.Database.Entities
{
    public class Track
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long TrackID { get; set; }
        public long ArtistID { get; set; }
        public long GenreID { get; set; }
        public long AlbumID { get; set; }
        public long FolderID { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long? FileSize { get; set; }
        public long? BitRate { get; set; }
        public long? SampleRate { get; set; }
        public string TrackTitle { get; set; }
        public long? TrackNumber { get; set; }
        public long? TrackCount { get; set; }
        public long? DiscNumber { get; set; }
        public long? DiscCount { get; set; }
        public long? Duration { get; set; }
        public long? Year { get; set; }
        public long? Rating { get; set; }
        public long? PlayCount { get; set; }
        public long? SkipCount { get; set; }
        public long DateAdded { get; set; }
        public long? DateLastPlayed { get; set; }
        public long DateLastSynced { get; set; }
        public long DateFileModified { get; set; }
        public string MetaDataHash { get; set; }

        [Ignore()]
        public string PathToLower { get; set; }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.PathToLower.Equals(((Track)obj).PathToLower);
        }

        public override int GetHashCode()
        {
            return new { this.PathToLower }.GetHashCode();
        }
        #endregion
    }
}
