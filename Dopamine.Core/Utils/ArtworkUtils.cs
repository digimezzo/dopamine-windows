using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Utils
{
    public static class ArtworkUtils
    {
        public static async Task<Uri> GetAlbumArtworkFromInternetAsync(string albumTitle, IList<string> albumArtists, string trackTitle = "", IList<string> trackArtists = null)
        {
            string title = string.Empty;
            List<string> artists = new List<string>();

            // Title
            if (!string.IsNullOrEmpty(albumTitle))
            {
                title = albumTitle;
            }
            else if (!string.IsNullOrEmpty(trackTitle))
            {
                title = trackTitle;
            }

            // Artist
            if (albumArtists != null && albumArtists.Count > 0)
            {
                artists.AddRange(albumArtists.Where(a => !string.IsNullOrEmpty(a)));
            }

            if (trackArtists != null && trackArtists.Count > 0)
            {
                artists.AddRange(trackArtists.Where(a => !string.IsNullOrEmpty(a)));
            }

            if (string.IsNullOrEmpty(title) || artists == null)
            {
                return null;
            }

            foreach (string artist in artists)
            {
                Api.Lastfm.Album lfmAlbum = await Api.Lastfm.LastfmApi.AlbumGetInfo(artist, title, false, "EN");

                if (!string.IsNullOrEmpty(lfmAlbum.LargestImage()))
                {
                    return new Uri(lfmAlbum.LargestImage());
                }
            }

            return null;
        }
    }
}
