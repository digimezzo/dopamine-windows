using Digimezzo.Utilities.Settings;
using Dopamine.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    // API from http://moonlib.com/606.html
    public class NeteaseLyricsApi : ILyricsApi
    {
        internal class LyricModel
        {
            public Lrc lrc { get; set; }
            public Lrc tlyric { get; set; }

            internal class Lrc
            {
                public int version { get; set; }
                public string lyric { get; set; }
            }
        }

        private ILocalizationInfo info;
        private const string apiSearchResultLimit = "1";

        private const string apiLyricsFormat = "song/lyric?os=pc&id={0}&lv=-1&tv=-1";

        private const string apiRootUrl = "http://music.163.com/api/";
        private int timeoutSeconds;
        private HttpClient httpClient;

        public NeteaseLyricsApi(int timeoutSeconds, ILocalizationInfo info)
        {
            this.timeoutSeconds = timeoutSeconds;
            this.info = info;

            httpClient = new HttpClient(new HttpClientHandler() {AutomaticDecompression = DecompressionMethods.GZip})
            {
                BaseAddress = new Uri(apiRootUrl)
            };
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,zh-CN;q=0.8,zh;q=0.6,en;q=0.4");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Referer", "http://music.163.com/");
            httpClient.DefaultRequestHeaders.Add("Host", "music.163.com");
            httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        }

        private async Task<string> ParseTrackIdAsync(string artist, string title)
        {
            var postContent = new[]
            {
                new KeyValuePair<string, string>("s", title + "\x20" + artist),
                new KeyValuePair<string, string>("offset", "0"),
                new KeyValuePair<string, string>("limit", apiSearchResultLimit),
                new KeyValuePair<string, string>("type", "1")
            };

            var response =
                await (await httpClient.PostAsync("search/pc", new FormUrlEncodedContent(postContent))).Content
                    .ReadAsStringAsync();

            int start = response.IndexOf("\",\"id\":") + 7;
            int end = response.IndexOf(",\"position\":", start);

            return response.Substring(start, end - start);
        }

        private async Task<string> ParseLyricsAsync(string trackId)
        {
            var resJson = await httpClient.GetStringAsync(String.Format(apiLyricsFormat, trackId));
            var res = JsonConvert.DeserializeObject<LyricModel>(resJson);
            if (string.IsNullOrEmpty(res.tlyric.lyric) || SettingsClient.Get<string>("Appearance", "Language") != "ZH-CN")
            {
                return res.lrc.lyric;
            }
            else
            {
                return res.tlyric.lyric;
            }
        }

        public string SourceName => this.info.NeteaseLyrics;

        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            var trackId = await ParseTrackIdAsync(artist, title);
            var result = await ParseLyricsAsync(trackId);

            return result;
        }
    }
}