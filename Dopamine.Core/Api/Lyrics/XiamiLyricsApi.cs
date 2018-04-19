using Dopamine.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public class XiamiLyricsApi : ILyricsApi
    {
        private const string ApiSearchResultLimit = "1";

        private const string ApiSearchFormat =
            "http://api.xiami.com/web?v=2.0&app_key=1&key={0}&page=1&limit=" + ApiSearchResultLimit +
            "&_ksTS=1489155872308_60&r=search/songs";
        private const string ApiTrackReferer =
            "http://www.xiami.com/play?ids=/song/playlist/id/{0}/object_name/default/object_id/0";
        private const string ApiTrackFormat =
            "http://www.xiami.com/song/playlist/id/{0}/object_name/default/object_id/0/cat/json?_ksTS={1}&callback={2}";
        private const int ApiKissyMagic = 377;

        private readonly ILocalizationInfo _info;
        private readonly int _timeoutSeconds;
        private HttpClient _httpClient;

        public XiamiLyricsApi(int timeoutSeconds, ILocalizationInfo info)
        {
            this._timeoutSeconds = timeoutSeconds;
            this._info = info;
        }

        private async Task<string> ParseTrackIdAsync(string artist, string title)
        {
            _httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            if (this._timeoutSeconds > 0) _httpClient.Timeout = TimeSpan.FromSeconds(this._timeoutSeconds);
            // Must set proper headers, otherwise the response will be "Illegal request"
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Host", "api.xiami.com");
            _httpClient.DefaultRequestHeaders.Add("Referer", "http://h.xiami.com/");
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36");

            var uri = new Uri(string.Format(ApiSearchFormat, title + "\x20" + artist));
            var resultString = await _httpClient.GetStringAsync(uri);
            var trackId = ParseSearchResultFromJson(resultString, artist, title);

            return trackId;
        }

        private string ParseSearchResultFromJson(string json, string artist, string title)
        {
            var start = json.IndexOf("[{\"song_id\":", StringComparison.Ordinal) + 12;
            var end = json.IndexOf(",\"song_name\"", start, StringComparison.Ordinal);
            var result = json.Substring(start, end - start);

            var totalStart = json.IndexOf("\"total\":", end, StringComparison.Ordinal) + 8;
            var totalEnd = json.IndexOf(",\"previous\":", totalStart, StringComparison.Ordinal);
            if (json.Substring(totalStart, totalEnd - totalStart) != "1")
            {
                // The offical api will return the completely incorrect response
                // even though the server doesn't contain this track
                start = json.IndexOf(":\"", end, StringComparison.Ordinal) + 2;
                end = json.IndexOf("\",\"", start, StringComparison.Ordinal);
                var resultTitle = Unicode2String(json.Substring(start, end - start));
                // Some tracks' title that API returns may contain useless blank character
                var length = title.Length < resultTitle.Length ? title.Length : resultTitle.Length;
                if (!title.Substring(0, length).Equals(resultTitle.Substring(0, length)))
                    result = string.Empty;
            }

            return result;
        }

        private async Task<string> ParseLyricsAsync(string trackId)
        {
            // Must remove following headers or the client will return Code 404
            _httpClient.DefaultRequestHeaders.Remove("Referer");
            _httpClient.DefaultRequestHeaders.Remove("Host");
            _httpClient.DefaultRequestHeaders.Remove("Accept");
            _httpClient.DefaultRequestHeaders.Add("Host", "www.xiami.com");
            _httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            _httpClient.DefaultRequestHeaders.Add("Referer", string.Format(ApiTrackReferer, trackId));

            var jsonp = $"jsonp{ApiKissyMagic + 1}";
            var uri = new Uri(string.Format(ApiTrackFormat, trackId,
                $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{ApiKissyMagic}", jsonp));

            var json = await _httpClient.GetStringAsync(uri);
            var trackInfo = JsonConvert.DeserializeObject<TrackModel>(json.Substring(jsonp.Length + 2, json.Length - jsonp.Length - 3));

            var lyricUrl = trackInfo.data.trackList[0].lyric_url;
            if (string.IsNullOrEmpty(lyricUrl))
            {
                throw new Exception("No Xiami Lyrics.");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("Referer");
                _httpClient.DefaultRequestHeaders.Remove("Host");

                return await _httpClient.GetStringAsync(lyricUrl);
            }
        }

        private static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        public string SourceName => this._info.XiamiLyrics;

        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            var trackId = await ParseTrackIdAsync(artist, title).ConfigureAwait(false);
            if (trackId.Equals(string.Empty)) throw new Exception("No Xiami Lyrics.");
            return await ParseLyricsAsync(trackId).ConfigureAwait(false);
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable CollectionNeverUpdated.Local
        private class TrackModel
        {
            public bool status { get; set; }
            public object message { get; set; }
            public Data data { get; set; }

            public class LyricInfo
            {
                public string lyricFile { get; set; }
            }

            public class TrackList
            {
                public LyricInfo lyricInfo { get; set; }
                public string lyric { get; set; }
                public string lyric_url { get; set; }
            }

            public class Data
            {
                public IList<TrackList> trackList { get; set; }
            }
        }
    }
}