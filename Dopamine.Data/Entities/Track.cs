using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    public class Track
    {
        [PrimaryKey(), AutoIncrement()]
        public long TrackID { get; set; }
        public long ArtistID { get; set; }
        public long GenreID { get; set; }
        public long AlbumID { get; set; }
        public string Path { get; set; }
        public string SafePath { get; set; }
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
        public long? HasLyrics { get; set; }
        public long DateAdded { get; set; }
        public long DateLastSynced { get; set; }
        public long DateFileModified { get; set; }
        public string MetaDataHash { get; set; }
        public long? NeedsIndexing { get; set; }
        public long? IndexingSuccess { get; set; }
        public string IndexingFailureReason { get; set; }
 
        public static Track CreateDefault(string path)
        {
            var track = new Track()
            {
                Path = path,
                SafePath = path.ToSafePath(),
                FileName = System.IO.Path.GetFileNameWithoutExtension(path),
                IndexingSuccess = 0,
                DateAdded = DateTime.Now.Ticks
        };

            return track;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.SafePath.Equals(((Track)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return new { this.SafePath }.GetHashCode();
        }
    }
}
