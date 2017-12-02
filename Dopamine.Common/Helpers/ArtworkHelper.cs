using Dopamine.Common.Base;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Helpers
{
    public static class ArtworkHelper
    {
        public static async Task<Uri> GetAlbumArtworkFromInternetAsync(string artist, string albumTitle)
        {
            if (artist.Equals(Defaults.UnknownArtistText) || albumTitle.Equals(Defaults.UnknownAlbumText))
            {
                return null;
            }

            Api.Lastfm.Album lfmAlbum = await Api.Lastfm.LastfmApi.AlbumGetInfo(artist, albumTitle, false, "EN");

            if (!string.IsNullOrEmpty(lfmAlbum.LargestImage()))
            {
                return new Uri(lfmAlbum.LargestImage());
            }

            return null;
        }
    }
}
