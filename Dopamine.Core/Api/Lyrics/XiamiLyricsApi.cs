using Dopamine.Core.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public class XiamiLyricsApi : ILyricsApi
    {
        private ILocalizationInfo info;
        private const string apiSearchResultLimit = "1";

        private const string apiSearchFormat =
            "http://api.xiami.com/web?v=2.0&app_key=1&key={0}&page=1&limit=" + apiSearchResultLimit +
            "&_ksTS=1489155872308_60&r=search/songs";

        private const string apiTrackFormat =
            "http://www.xiami.com/song/playlist/id/{0}/object_name/default/object_id/0/cat/json";

        private const string apiTrackDetailWebPageFormat = "http://www.xiami.com/song/{0}";
        private int timeoutSeconds;
        private HttpClient httpClient;

        public XiamiLyricsApi(int timeoutSeconds, ILocalizationInfo info)
        {
            this.timeoutSeconds = timeoutSeconds;
            this.info = info;
        }

        private async Task<string> ParseTrackIdAsync(string artist, string title)
        {
            string trackId = string.Empty;

            await Task.Run(async () =>
            {
                httpClient = new HttpClient(new HttpClientHandler() {AutomaticDecompression = DecompressionMethods.GZip});
                if (this.timeoutSeconds > 0) httpClient.Timeout = TimeSpan.FromSeconds(this.timeoutSeconds);
                // Must set proper headers, otherwise the response will be "Illegal request"
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,zh-CN;q=0.8,zh;q=0.6,en;q=0.4");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.Add("Host", "api.xiami.com");
                httpClient.DefaultRequestHeaders.Add("Referer", "http://h.xiami.com/");
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

                Uri uri = new Uri(String.Format(apiSearchFormat, title + "\x20" + artist));
                var resultString = await httpClient.GetStringAsync(uri);
                trackId = await ParseSearchResultFromJsonAsync(resultString, artist, title);
            });

            return trackId;
        }

        private async Task<string> ParseSearchResultFromJsonAsync(string json, string artist, string title)
        {
            string result = string.Empty;

            await Task.Run(async () =>
            {
                int start = json.IndexOf("[{\"song_id\":") + 12;
                int end = json.IndexOf(",\"song_name\"", start);
                result = json.Substring(start, end - start);

                int totalStart = json.IndexOf("\"total\":", end) + 8;
                int totalEnd = json.IndexOf(",\"previous\":", totalStart);
                if (json.Substring(totalStart, totalEnd - totalStart) != "1")
                {
                    // The offical api will return the completely incorrect response
                    // even though the server doesn't contain this track
                    start = json.IndexOf(":\"", end) + 2;
                    end = json.IndexOf("\",\"", start);
                    string resultTitle = await Unicode2String(json.Substring(start, end - start));
                    // Some tracks' title that API returns may contain useless blank character
                    int length = title.Length < resultTitle.Length ? title.Length : resultTitle.Length;
                    if (!title.Substring(0, length).Equals(resultTitle.Substring(0, length)))
                        result = string.Empty;
                }
            });

            return result;
        }

        private async Task<Tuple<bool, string>> ParseLyrcisUrlThroughApiAsync(string trackId)
        {
            var result = new Tuple<bool, string>(false, null);

            await Task.Run(async () =>
            {
                // Must remove following headers or the client will return Code 404
                httpClient.DefaultRequestHeaders.Remove("Referer");
                httpClient.DefaultRequestHeaders.Remove("Host");
                httpClient.DefaultRequestHeaders.Remove("Accept");
                httpClient.DefaultRequestHeaders.Add("Host", "www.xiami.com");
                httpClient.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                var uri = new Uri(String.Format(apiTrackFormat, trackId));

                string json = await httpClient.GetStringAsync(uri);

                int start = json.IndexOf(",\"lyric_url\":\"http:\\/\\/");
                int end = json.IndexOf("\",\"object_id\":", start);
                if (start > 0 && end > 0)
                    start += 23;
                var sb = new StringBuilder(json.Substring(start, end - start)).Replace(@"\/", @"/");
                result = new Tuple<bool, string>(true, "http://" + sb.ToString());
            });

            return result;
        }

        private async Task<Tuple<bool, string>> ParseLyricsAsync(string trackId)
        {
            var result = new Tuple<bool, string>(false, null);

            // If this track is delisted we cannot get the dynamic lyrics without logging,
            // try to fetch plain text lyrics
            var tempResult = await ParseLyrcisUrlThroughApiAsync(trackId);
            if (tempResult.Item1 == true)
            {
                httpClient.DefaultRequestHeaders.Remove("Host");
                httpClient.DefaultRequestHeaders.Add("Host", "img.xiami.net");

                result = new Tuple<bool, string>(true, await httpClient.GetStringAsync(tempResult.Item2));
            }
            else
            {
                Uri uri = new Uri(String.Format(apiTrackDetailWebPageFormat, trackId));
                string html = await httpClient.GetStringAsync(uri);

                await Task.Run(() =>
                {
                    int start = html.IndexOf("<div class=\"lrc_main\">");
                    int end = html.IndexOf("</div>", start);
                    if (start > 0 && end > 0)
                    {
                        start += 22;
                        var sb = new StringBuilder(html.Substring(start, end - start));
                        result = new Tuple<bool, string>(true, sb.Replace("<br />", "").Replace("\t", "").ToString());
                    }
                });
            }

            return result;
        }

        private async Task<string> Unicode2String(string source)
        {
            string result = string.Empty;

            await Task.Run(
                () => result = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                    source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16))));

            return result;
        }

        public string SourceName => this.info.XiamiLyrics;

        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            var trackId = await ParseTrackIdAsync(artist, title);
            if (trackId.Equals(string.Empty)) throw new Exception("No Xiami Lyrics.");
            var result = await ParseLyricsAsync(trackId);
            if (result.Item1 == false) throw new Exception("No Xiami Lyrics.");
            return result.Item2;
        }
    }
}