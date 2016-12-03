using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lyrics
{
    public class ChartLyricsApi : ILyricsApi
    {
        #region Variables
        private const string apiRootFormat = "http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist={0}&song={1}";
        #endregion

        #region Private
        public async Task<string> ParseResultAsync(string result)
        {
            string lyrics = string.Empty;

            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // http://www.chartlyrics.com/api.aspx
                    var resultXml = XDocument.Parse(result);

                    // Select elements by LocalName because ChartLyrics XML has namespace issues
                    // We know the element we want is uniquely named, so we skip all the intermediate elements
                    lyrics = (from t in resultXml.Root.Descendants().Where(e => e.Name.LocalName == "Lyric")
                              select t.Value).FirstOrDefault();
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
                return "ChartLyrics";
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
