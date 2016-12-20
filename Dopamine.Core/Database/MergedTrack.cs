using Dopamine.Core.Base;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Extensions;
using System.Collections.Generic;

namespace Dopamine.Core.Database
{
    public class MergedTrack : Track
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
        #endregion

        #region Static
        public static MergedTrack CreateDefault(string path)
        {
            var track = new MergedTrack();

            track.Path = path;
            track.SafePath = path.ToSafePath();
            track.FileName = System.IO.Path.GetFileNameWithoutExtension(path);

            track.ArtistName = Defaults.UnknownArtistString;

            track.GenreName = Defaults.UnknownGenreString;

            track.AlbumTitle = Defaults.UnknownAlbumString;
            track.AlbumArtist = Defaults.UnknownAlbumArtistString;

            return track;
        }
        #endregion

        #region Public
        public List<MergedTrack> ToList()
        {

            List<MergedTrack> l = new List<MergedTrack>();
            l.Add(this);

            return l;
        }
        #endregion
    }
}
