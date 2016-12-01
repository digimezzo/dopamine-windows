using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public class LyricsFactory
    {
        #region Variables
        private LyricWikiaApi lyricWikiaApi;
        private LololyricsApi lololyricsApi;
        private ChartLyricsApi chartLyricsApi;
        #endregion

        #region COnstruction
        public LyricsFactory()
        {
            lyricWikiaApi = new LyricWikiaApi();
            lololyricsApi = new LololyricsApi();
            chartLyricsApi = new ChartLyricsApi();
        }
        #endregion

        #region Public
        public async Task<Lyrics> GetLyricsAsync(string artist, string title)
        {
            Lyrics lyrics = null;

            lyrics = new Lyrics( await lyricWikiaApi.GetLyricsAsync(artist, title), "LyricWikia");
            if (!lyrics.HasText) lyrics = new Lyrics(await lololyricsApi.GetLyricsAsync(artist, title),"LoloLyrics");
            if (!lyrics.HasText) lyrics = new Lyrics(await chartLyricsApi.GetLyricsAsync(artist, title),"ChartLyrics");

            return lyrics;
        }
        #endregion
    }
}
