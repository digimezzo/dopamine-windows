using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Common.Api.Lyrics
{
    public class LololyricsApi : ILyricsApi
    {
        #region Variables
        private const string apiRootFormat = "http://api.lololyrics.com/0.5/getLyric?artist={0}&track={1}";
        private int timeoutSeconds;
        #endregion

        #region Construction
        public LololyricsApi(int timeoutSeconds)
        {
            this.timeoutSeconds = timeoutSeconds;
        }
        #endregion

        #region Private
        public async Task<string> ParseResultAsync(string result)
        {
            string lyrics = string.Empty;

            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // http://api.lololyrics.com/
                    var resultXml = XDocument.Parse(result);

                    // Status
                    string status = (from t in resultXml.Element("result").Elements("status")
                                     select t.Value).FirstOrDefault();

                    if (status != null && status.ToLower() == "ok")
                    {
                        lyrics = (from t in resultXml.Element("result").Elements("response")
                                  select t.Value).FirstOrDefault();
                    }
                }
            });

            return lyrics;
        }
        #endregion

        #region ILyricsApi
        public string SourceName
        {
            get
            {
                return "LoloLyrics";
            }
        }

        /// <summary>
        /// Searches for lyrics for the given artist and title
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            Uri uri = new Uri(string.Format(apiRootFormat, artist, title));

            string result = string.Empty;

            using (var client = new HttpClient())
            {
                if (this.timeoutSeconds > 0) client.Timeout = TimeSpan.FromSeconds(this.timeoutSeconds);
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.GetAsync(uri);
                result = await response.Content.ReadAsStringAsync();
            }

            string lyrics = await ParseResultAsync(result);

            return lyrics;
        }
        #endregion
    }
}