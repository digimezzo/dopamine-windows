using Digimezzo.Utilities.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Common.Api.Lyrics
{
    public class MetroLyricsApi : ILyricsApi
    {
        #region Variables
        private const string apiRootFormat = "http://www.metrolyrics.com/{0}-lyrics-{1}.html";
        private int timeoutSeconds;
        #endregion

        #region Construction
        public MetroLyricsApi(int timeoutSeconds)
        {
            this.timeoutSeconds = timeoutSeconds;
        }
        #endregion

        #region Private
        /// <summary>
        /// The url must have the format: http://www.metrolyrics.com/teardrop-lyrics-massive-attack.html
        /// All parts must be lowercase and split by "-"
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task<string> BuildUrlAsync(string artist, string title)
        {
            string url = string.Empty;

            await Task.Run(() =>
            {
                string[] artistPieces = artist.ToLower().Split(' ');
                string[] titlePieces = title.ToLower().Split(' ');

                string joinedArtist = string.Join("-", artistPieces.Select(p => StringUtils.FirstCharToUpper(p)));
                string joinedTitle = string.Join("-", titlePieces.Select(p => StringUtils.FirstCharToUpper(p)));

                url = string.Format(apiRootFormat, joinedTitle, joinedArtist);
            });

            return url;
        }

        private async Task<string> ParseLyricsFromHtmlAsync(string html, string originalArtist, string originalTitle)
        {
            string lyrics = string.Empty;

            await Task.Run(() =>
            {
                int start = html.IndexOf("<div id=\"lyrics-body-text\" class=\"js-lyric-text\">");
                int end = html.IndexOf("</div>", start);

                if (start > 0 && end > 0)
                {
                    int correctedStart = start + 49;
                    int correctedEnd = end;
                    lyrics = html.Substring(correctedStart, correctedEnd - correctedStart).Replace("<p class='verse'>", "").Replace("</p>", Environment.NewLine + Environment.NewLine).Replace("<br>", "").Trim();
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
                return "MetroLyrics";
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
            Uri uri = new Uri(await BuildUrlAsync(artist, title));

            string result = string.Empty;

            using (var client = new HttpClient())
            {
                if (this.timeoutSeconds > 0) client.Timeout = TimeSpan.FromSeconds(this.timeoutSeconds);
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.GetAsync(uri);
                result = await response.Content.ReadAsStringAsync();
            }

            string lyrics = await ParseLyricsFromHtmlAsync(result, artist, title);

            return lyrics;
        }
        #endregion
    }
}
