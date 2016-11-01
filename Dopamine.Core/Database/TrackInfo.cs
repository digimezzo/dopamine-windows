using Dopamine.Core.Database.Entities;
using System.Collections.Generic;

namespace Dopamine.Core.Database
{
    public class TrackInfo
    {
        #region Track
        public long TrackID { get; set; }
        public long ArtistID { get; set; }
        public long GenreID { get; set; }
        public long AlbumID { get; set; }
        public long FolderID { get; set; }
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
        public long? Rating { get; set; }
        public long? Love { get; set; }
        public long? PlayCount { get; set; }
        public long? SkipCount { get; set; }
        public long DateAdded { get; set; }
        public long? DateLastPlayed { get; set; }
        public long DateLastSynced { get; set; }
        public long DateFileModified { get; set; }
        public string MetaDataHash { get; set; }
        #endregion

        #region Artist
        public string ArtistName { get; set; }
        #endregion

        #region Genre
        public string GenreName { get; set; }
        #endregion

        #region Album
        public string AlbumTitle { get; set; }
        public string AlbumArtist { get; set; }
        public long? AlbumYear { get; set; }
        public string AlbumArtworkID { get; set; }
        #endregion

        #region Public
        public List<TrackInfo> ToList()
        {

            List<TrackInfo> l = new List<TrackInfo>();
            l.Add(this);

            return l;
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            return this.SafePath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.SafePath.Equals(((TrackInfo)obj).SafePath);
        }
        #endregion
    }
}
