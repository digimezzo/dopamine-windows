using Dopamine.Core.Base;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Extensions;
using System.Collections.Generic;

namespace Dopamine.Core.Database
{
    public class PlayableTrack : Track
    {
        #region TrackStastistic
        public long? Rating { get; set; }
        public long? Love { get; set; }
        public long? PlayCount { get; set; }
        public long? SkipCount { get; set; }
        public long? DateLastPlayed { get; set; }
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
        #endregion

        #region Static
        public static PlayableTrack CreateDefault(string path)
        {
            var track = new PlayableTrack();

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
        public List<PlayableTrack> ToList()
        {
            List<PlayableTrack> l = new List<PlayableTrack>();
            l.Add(this);

            return l;
        }
        #endregion
    }
}
