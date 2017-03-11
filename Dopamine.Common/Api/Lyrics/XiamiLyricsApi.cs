using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dopamine.Common.Api.Lyrics
{
    public class XiamiLyricsApi : ILyricsApi
    {
        #region Variables

        private const string apiSearchResultLimit = "1";

        private const string apiSearchFormat =
            "http://api.xiami.com/web?v=2.0&app_key=1&key={0}&page=1&limit=" + apiSearchResultLimit +
            "&_ksTS=1489155872308_60&r=search/songs";

        private const string apiTrackFormat =
            "http://www.xiami.com/song/playlist/id/{0}/object_name/default/object_id/0/cat/json";

        private const string apiTrackDetailWebPageFormat = "http://www.xiami.com/song/{0}";
        private int timeoutSeconds;
        private HttpClient httpClient;

        #endregion

        #region Construction

        public XiamiLyricsApi(int timeoutSeconds)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        #endregion

        #region Private

        private async Task<string> ParseTrackIdAsync(string artist, string title)
        {
            string trackId = string.Empty;

            await Task.Run(async () =>
            {
                httpClient = new HttpClient();
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

            await Task.Run(() =>
            {
                int start = json.IndexOf("[{\"song_id\":") + 12;
                int end = json.IndexOf(",\"song_name\"", start);
                result = json.Substring(start, end - start);

                if (json[json.IndexOf("\"total\":") + 8] != '1')
                {
                    // The offical api will return the completely incorrect response
                    // even though the server doesn't contain this track
                    start = json.IndexOf(":\"", end) + 2;
                    end = json.IndexOf("\",\"", start);
                    string resultTitle = Unicode2String(json.Substring(start, end - start));
                    // Some tracks' title that API returns may contain useless blank character
                    int length = title.Length < resultTitle.Length ? title.Length : resultTitle.Length;
                    if (!title.Substring(0, length).Equals(resultTitle.Substring(0, length)))
                        result = string.Empty;
                }
            });

            return result;
        }

        private async Task<string> ParseTrackDetailAsync(string trackId)
        {
            string result = string.Empty;

            await Task.Run(async () =>
            {
                // Must remove following headers or the client will return Code 404
                httpClient.DefaultRequestHeaders.Remove("Referer");
                httpClient.DefaultRequestHeaders.Remove("Host");
                httpClient.DefaultRequestHeaders.Remove("Accept");
                httpClient.DefaultRequestHeaders.Add("Host", "www.xiami.com");
                httpClient.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                bool isSuccessful = true;

                var uri = new Uri(String.Format(apiTrackFormat, trackId));
                var response = await httpClient.GetStreamAsync(uri);
                string json = await DecompressStringFromGzip(response);

                int start = json.IndexOf(",\"lyric_url\":\"http:\\/\\/");
                int end = json.IndexOf("\",\"object_id\":", start);
                if (start <=0 || end <= 0)
                    throw new Exception("No lrc found.");
                start += 23;
                var sb = new StringBuilder(json.Substring(start, end - start)).Replace(@"\/", @"/");
                result = "http://" + sb.ToString();
            });

            return result;
        }

        private async Task<string> ParseLyricsAsync(string trackId)
        {
            string lyrics = string.Empty;
            // If this track is delisted we cannot get the dynamic lyrics without logging,
            // try to fetch plain text lyrics
            try
            {
                var lrcUrl = await ParseTrackDetailAsync(trackId);

                httpClient.DefaultRequestHeaders.Remove("Host");
                httpClient.DefaultRequestHeaders.Add("Host", "img.xiami.net");

                lyrics = await httpClient.GetStringAsync(lrcUrl);
            }
            catch (Exception)
            {
                Uri uri = new Uri(String.Format(apiTrackDetailWebPageFormat, trackId));
                var response = await httpClient.GetStreamAsync(uri);
                string html = await DecompressStringFromGzip(response);

                await Task.Run(() =>
                {
                    int start = html.IndexOf("<div class=\"lrc_main\">");
                    int end = html.IndexOf("</div>", start);
                    if (start <= 0 || end <= 0)
                        throw new Exception("No lrc found.");
                    start += 22;
                    var sb = new StringBuilder(html.Substring(start, end - start));
                    lyrics = sb.Replace("<br />", "").Replace("\t", "").ToString();
                });
            }

            return lyrics;
        }

        private string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        private async Task<string> DecompressStringFromGzip(Stream stream)
        {
            string result = string.Empty;

            await Task.Run(async () =>
            {
                using (var gs = new GZipStream(stream, CompressionMode.Decompress))
                using (var ms = new MemoryStream())
                {
                    await gs.CopyToAsync(ms);
                    result = Encoding.UTF8.GetString(ms.ToArray());
                }
            });

            return result;
        }

        #endregion

        #region ILyricsApi

        public string SourceName => "XiamiLyrics";

        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            string trackId = await ParseTrackIdAsync(artist, title);
            if (trackId.Equals(string.Empty)) throw new Exception("No Xiami Lyrics.");
            string lyrcis = await ParseLyricsAsync(trackId);
            return lyrcis;
        }

        #endregion
    }
}