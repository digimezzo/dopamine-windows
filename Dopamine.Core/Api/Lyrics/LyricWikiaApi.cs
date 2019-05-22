using Digimezzo.Foundation.Core.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    /// <summary>
    /// LyricWikia doesn't have a proper API. So we need to search using the URL.
    /// The "edit" URL is case-sensitive. If casing of artist and track do not correspond,
    /// no lyrics are returned. Even if there are lyrics available.
    /// </summary>
    public class LyricWikiaApi : ILyricsApi
    {
        private const string apiRootFormat = "http://lyrics.wikia.com/wiki/{0}:{1}?action=edit";
        private int timeoutSeconds;

        public LyricWikiaApi(int timeoutSeconds)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// The url must have the format: http://lyrics.wikia.com/wiki/Massive_Attack:Teardrop?action=edit
        /// Capitalization of the first letter of all words is important. It doesn't find lyrics without it.
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task<string> BuildUrlAsync(string artist, string title, bool correctCasing)
        {
            string url = string.Empty;

            await Task.Run(() =>
            {
                // Split artist and title into pieces based on space " "
                string[] artistPieces = (correctCasing ? artist.ToLower() : artist).Split(' ');
                string[] titlePieces = (correctCasing ? title.ToLower() : title).Split(' ');

                // Join artist and title pieces using "_"
                string joinedArtist = string.Join("_", artistPieces.Select(p => correctCasing ? StringUtils.FirstCharToUpper(p) : p));
                string joinedTitle = string.Join("_", titlePieces.Select(p => correctCasing ? StringUtils.FirstCharToUpper(p) : p));

                // Replace special characters
                joinedArtist = joinedArtist.Replace("?", "%3F");
                joinedTitle = joinedTitle.Replace("?", "%3F");
                joinedArtist = joinedArtist.Replace("'", "%27");
                joinedTitle = joinedTitle.Replace("'", "%27");

                url = string.Format(apiRootFormat, joinedArtist, joinedTitle);
            });

            return url;
        }

        private async Task<string> ParseLyricsFromHtmlAsync(string html, string originalArtist, string originalTitle)
        {
            string lyrics = string.Empty;

            int start;
            int end;

            // We were redirected
            if (html.Contains("#REDIRECT"))
            {
                string artist = string.Empty;
                string title = string.Empty;

                await Task.Run(() =>
                {
                    start = html.IndexOf("#REDIRECT [[") + 12;
                    end = html.IndexOf("]]", start);

                    artist = html.Substring(start, end - start).Split(':')[0];
                    title = html.Substring(start, end - start).Split(':')[1];
                });

                // We don't want to perform a redirect if we're proposed the 
                // same artist and title That would cause an infinite loop here.
                if (artist != originalArtist || title != originalTitle) lyrics = await GetLyricsAsync(artist, title);
            }
            // No lyrics found
            else if (html.Contains("!-- PUT LYRICS HERE (and delete this entire line) -->"))
            {
                lyrics = string.Empty;
            }
            // Lyrics found
            else
            {
                await Task.Run(() =>
                {
                    start = html.IndexOf("&lt;lyrics>");
                    end = html.IndexOf("&lt;/lyrics>");

                    if (start > 0 && end > 0)
                    {
                        int correctedStart = start + 12;
                        int correctedEnd = end - 1;

                        // Replace webpage newline with carriage return + newline (standard on Windows)
                        lyrics = html.Substring(correctedStart, correctedEnd - correctedStart).Replace("\n", Environment.NewLine);
                    }
                });
            }

            return lyrics;
        }

        public string SourceName
        {
            get
            {
                return "LyricWikia";
            }
        }

        private async Task<string> GetLyricsFromWeb(Uri uri, string artist, string title)
        {
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

        /// <summary>
        /// Searches for lyrics for the given artist and title
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            // First, try without casing correction.
            Uri uri = new Uri(await BuildUrlAsync(artist, title, false));
            string lyrics = await this.GetLyricsFromWeb(uri, artist, title);

            // If no lyrics were returned
            if (string.IsNullOrEmpty(lyrics))
            {
                // Try again, with casing correction.
                uri = new Uri(await BuildUrlAsync(artist, title, true));
                lyrics = await this.GetLyricsFromWeb(uri, artist, title);
            }

            return lyrics;
        }
    }
}
