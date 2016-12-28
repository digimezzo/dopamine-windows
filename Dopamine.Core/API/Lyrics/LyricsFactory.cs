using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public class LyricsFactory
    {
        #region Variables
        private List<ILyricsApi> lyricsApis;
        #endregion

        #region COnstruction
        public LyricsFactory()
        {
            lyricsApis = new List<ILyricsApi>();

            lyricsApis.Add(new LyricWikiaApi());
            lyricsApis.Add(new LololyricsApi());
            lyricsApis.Add(new ChartLyricsApi());
        }
        #endregion

        #region Public
        public async Task<Lyrics> GetLyricsAsync(string artist, string title)
        {
            Lyrics lyrics = null;
            ILyricsApi api = this.GetRandomApi();

            while (api != null && (lyrics == null || !lyrics.HasText))
            {
                try
                {
                    lyrics = new Lyrics(await api.GetLyricsAsync(artist, title), api.SourceName);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting lyrics from '{0}'. Exception: {1}", api.SourceName, ex.Message);
                }
                
                api = this.GetRandomApi();
            }

            return lyrics;
        }
        #endregion

        #region Private
        private ILyricsApi GetRandomApi()
        {
            ILyricsApi api = null;

            if (lyricsApis.Count > 0)
            {
                var rnd = new Random();
                int index = rnd.Next(lyricsApis.Count);
                api = lyricsApis[index];
                lyricsApis.RemoveAt(index);
            }

            return api;
        }
        #endregion
    }
}
