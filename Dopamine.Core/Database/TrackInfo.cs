using Dopamine.Core.Database.Entities;
using System.Collections.Generic;

namespace Dopamine.Core.Database
{
    public class TrackInfo : Track
    {
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
    }
}
