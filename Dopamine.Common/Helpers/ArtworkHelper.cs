using Dopamine.Common.Base;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Helpers
{
    public static class ArtworkHelper
    {
        public static async Task<Uri> GetAlbumArtworkFromInternetAsync(string title, string artist, string alternateTitle = "", string alternateArtist = "")
        {
            string albumTitle = string.Empty;
            string albumArtist = string.Empty;

            // Title
            if (!title.Equals(Defaults.UnknownAlbumText) && !string.IsNullOrEmpty(title))
            {
                albumTitle = title;
            }
            else if (!alternateTitle.Equals(Defaults.UnknownAlbumText) && !string.IsNullOrEmpty(alternateTitle))
            {
                albumTitle = alternateTitle;
            }

            // Artist
            if (!artist.Equals(Defaults.UnknownAlbumText) && !string.IsNullOrEmpty(artist))
            {
                albumArtist = artist;
            }
            else if (!alternateArtist.Equals(Defaults.UnknownAlbumText) && !string.IsNullOrEmpty(alternateArtist))
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
