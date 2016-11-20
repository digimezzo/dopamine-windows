using Dopamine.Core.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.LyricWikia
{
    /// <summary>
    /// Idea borrowed from here: http://inversekarma.in/technology/net/fetching-lyrics-from-lyricwiki-in-/c
    /// </summary>
    public static class LyricWikiaApi
    {
        private const string apiRootFormat = "http://lyrics.wikia.com/wiki/{0}:{1}?action=edit";

        /// <summary>
        /// The url must have the format: http://lyrics.wikia.com/wiki/Massive_Attack:Teardrop?action=edit
        /// Capitalization of the first letter of all words is important. It doesn't find lyrics without it.
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static async Task<string> BuildUrlAsync(string artist, string title)
        {
            string url = string.Empty;

            await Task.Run(() =>
            {
                string[] artistPieces = artist.ToLower().Split(' ');
                string[] titlePieces = title.ToLower().Split(' ');

                string joinedArtist = string.Join("_", artistPieces.Select(p => StringUtils.FirstCharToUpper(p)));
                string joinedTitle = string.Join("_", titlePieces.Select(p => StringUtils.FirstCharToUpper(p)));

                url = string.Format(apiRootFormat, joinedArtist, joinedTitle);
            });

            return url;
        }

        private static async Task<string> ParseLyricsFromHtmlAsync(string html)
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

                lyrics = await GetLyricsAsync(artist, title);
            }
            // No lyrics found
            else if(html.Contains("!-- PUT LYRICS HERE (and delete this entire line) -->"))
            {
                lyrics = string.Empty;
            }
            // Lyrics found
            else
            {
                await Task.Run(() =>
                {
                    start = html.IndexOf("&lt;lyrics>") + 12;
                    end = html.IndexOf("&lt;/lyrics>") - 1;

                    // Replace webpage newline with carriage return + newline (standard on Windows)
                    lyrics = html.Substring(start, end - start).Replace("\n", Environment.NewLine);
                });
            }

            return lyrics;
        }

        /// <summary>
        /// Searches for lyrics for the given artist and title
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<string> GetLyricsAsync(string artist, string title)
        {
            string url = Uri.EscapeUriString(await BuildUrlAsync(artist, title));

            string result = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.GetAsync(url);
                result = await response.Content.ReadAsStringAsync();
            }

            string lyrics = await ParseLyricsFromHtmlAsync(result);

            return lyrics;
        }
    }
}
