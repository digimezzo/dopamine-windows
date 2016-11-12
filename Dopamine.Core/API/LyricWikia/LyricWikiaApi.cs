using Dopamine.Core.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.LyricWikia
{
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

        private static async Task<string> ParseLyricsFromHtml(string html)
        {
            string lyrics = null;

            await Task.Run(() =>
            {
                // Get start and end index
                int start = html.IndexOf("&lt;lyrics>") + 12;
                int end = html.IndexOf("&lt;/lyrics>") - 1;

                // Replace webpage newline with carriage return + newline (standard on Windows)
                lyrics = html.Substring(start, end - start).Replace("\n", Environment.NewLine);

                // No lyrics found
                if (lyrics.Contains("!-- PUT LYRICS HERE (and delete this entire line) -->")) lyrics = string.Empty;
            });

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

            string lyrics = await ParseLyricsFromHtml(result);

            return lyrics;
        }
    }
}
