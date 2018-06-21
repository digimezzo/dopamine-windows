using Dopamine.Core.Base;
using System;
using System.Threading.Tasks;

namespace Dopamine.Core.Utils
{
    public static class ArtworkUtils
    {
        public static async Task<Uri> GetAlbumArtworkFromInternetAsync(string title, string artist, string alternateTitle = "", string alternateArtist = "")
        {
            string albumTitle = string.Empty;
            string albumArtist = string.Empty;

            // Title
            if (!string.IsNullOrEmpty(title))
            {
                albumTitle = title;
            }
            else if (!string.IsNullOrEmpty(alternateTitle))
            {
                albumTitle = alternateTitle;
            }

            // Artist
            if (!string.IsNullOrEmpty(artist))
            {
                albumArtist = artist;
            }
            else if (!string.IsNullOrEmpty(alternateArtist))
            {
                albumArtist = alternateArtist;
            }

            if (string.IsNullOrEmpty(albumTitle) || string.IsNullOrEmpty(albumArtist))
            {
                return null;
            }

            Api.Lastfm.Album lfmAlbum = await Api.Lastfm.LastfmApi.AlbumGetInfo(albumArtist, albumTitle, false, "EN");

            if (!string.IsNullOrEmpty(lfmAlbum.LargestImage()))
            {
                return new Uri(lfmAlbum.LargestImage());
            }

            return null;
        }
    }
}
