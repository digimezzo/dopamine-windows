using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public class LyricsFactory
    {
        #region Variables
        private LyricWikiaApi lyricWikiaApi;
        private LololyricsApi lololyricsApi;
        #endregion

        #region COnstruction
        public LyricsFactory()
        {
            lyricWikiaApi = new LyricWikiaApi();
            lololyricsApi = new LololyricsApi();
        }
        #endregion

        #region Public
        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            string lyrics = string.Empty;

            lyrics = await lyricWikiaApi.GetLyricsAsync(artist, title);
            if(string.IsNullOrWhiteSpace(lyrics)) lyrics = await lololyricsApi.GetLyricsAsync(artist, title);

            return lyrics;
        }
        #endregion
    }
}
