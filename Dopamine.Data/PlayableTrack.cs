using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Entities;
using System.Collections.Generic;

namespace Dopamine.Data
{
    public class PlayableTrack : Track
    {
        public long? Rating { get; set; }
        public long? Love { get; set; }
        public long? PlayCount { get; set; }
        public long? SkipCount { get; set; }
        public long? DateLastPlayed { get; set; }
        public string ArtistName { get; set; }
        public string GenreName { get; set; }
        public string AlbumTitle { get; set; }
        public string AlbumArtist { get; set; }
        public long? AlbumYear { get; set; }

        public static PlayableTrack CreateDefault(string path)
        {
            var track = new PlayableTrack();

            track.Path = path;
            track.SafePath = path.ToSafePath();
            track.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            track.ArtistName = Defaults.UnknownArtistText;
            track.GenreName = Defaults.UnknownGenreText;
            track.AlbumTitle = Defaults.UnknownAlbumText;
            track.AlbumArtist = Defaults.UnknownArtistText;

            return track;
        }

        public List<PlayableTrack> ToList()
        {
            List<PlayableTrack> l = new List<PlayableTrack>();
            l.Add(this);

            return l;
        }
    }
}
