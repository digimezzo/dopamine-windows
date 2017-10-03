using Digimezzo.Utilities.Utils;

namespace Dopamine.Common.Helpers
{
    public class LocalizationInfo : ILocalizationInfo
    {
        public string UnknownArtistText => ResourceUtils.GetString("Language_Unknown_Artist");
        public string UnknownGenreText => ResourceUtils.GetString("Language_Unknown_Genre");
        public string UnknownAlbumText => ResourceUtils.GetString("Language_Unknown_Album");
        public string NeteaseLyrics => ResourceUtils.GetString("Language_NeteaseLyrics");
        public string XiamiLyrics => ResourceUtils.GetString("Language_XiamiLyrics");
    }
}
