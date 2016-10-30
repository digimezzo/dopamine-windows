using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.API.Lastfm
{
    public static class LastfmAPI
    {
        #region Private
        /// <summary>
        /// Performs a POST request over HTTP or HTTPS
        /// </summary>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="useHttps"></param>
        /// <returns></returns>
        private static async Task<string> PerformPostRequestAsync(string method, NameValueCollection data, bool useHttps)
        {
            string prefix = "http";
            if (useHttps) prefix = "https";

            string result = string.Empty;

            string url = Uri.EscapeUriString(string.Format("{0}://ws.audioscrobbler.com/2.0/?method={1}", prefix, method));

            using (var client = new WebClient())
            {
                byte[] responseBytes = await client.UploadValuesTaskAsync(url, "POST", data);
                result = Encoding.UTF8.GetString(responseBytes);
            }

            return result;
        }

        /// <summary>
        /// Performs a GET request over HTTP or HTTPS
        /// </summary>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="useHttps"></param>
        /// <returns></returns>
        private static async Task<string> PerformGetRequestAsync(string method, NameValueCollection data, bool useHttps)
        {
            string prefix = "http";
            if (useHttps) prefix = "https";

            string result = string.Empty;

            var dataList = new List<string>();

            // Add everything to the list
            foreach (string key in data)
            {
                dataList.Add(string.Format("{0}={1}", key, data[key]));
            }

            dataList.Add("method=" + method);

            string url = Uri.EscapeUriString(string.Format("{0}://ws.audioscrobbler.com/2.0/?{1}", prefix, string.Join("&", dataList.ToArray())));

            using (var client = new WebClient())
            {
                byte[] responseBytes = await client.DownloadDataTaskAsync(url);
                result = Encoding.UTF8.GetString(responseBytes);
            }

            return result;
        }

        /// <summary>
        /// Constructs an API method signature as described in point 4 on http://www.last.fm/api/mobileauth
        /// </summary>
        /// <param name="data"></param>
        /// <param name="method"></param>
        /// <returns>API method signature</returns>
        private static string GenerateSignature(NameValueCollection data, string method)
        {
            var alphabeticalList = new List<string>();

            // Add everything to the list
            foreach (string key in data)
            {
                alphabeticalList.Add(string.Format("{0}{1}", key, data[key]));
            }

            alphabeticalList.Add("method" + method);

            // Order the list alphabetically
            alphabeticalList = alphabeticalList.OrderBy((t) => t).ToList();


            // Join all parts of the list alphabetically and append API secret
            string signatureString = string.Format("{0}{1}", string.Join("", alphabeticalList.ToArray()), SensitiveInformation.LastfmSharedSecret);

            // Create MD5 hash and return that
            return CryptographyUtils.MD5Hash(signatureString);
        }
        #endregion

        #region Public
        /// <summary>
        /// Requests authorization from the user by sending a POST request over HTTPS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Session key</returns>
        public static async Task<string> GetMobileSession(string username, string password)
        {
            string method = "auth.getMobileSession";

            var data = new NameValueCollection();
            data["username"] = username;
            data["password"] = password;
            data["api_key"] = SensitiveInformation.LastfmApiKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequestAsync(method, data, true);

            // If the status of the result is ok, get the session key.
            string sessionKey = string.Empty;

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/auth.getMobileSession
                var resultXml = XDocument.Parse(result);

                // Status
                string lfmStatus = (from t in resultXml.Elements("lfm")
                                    select t.Attribute("status").Value).FirstOrDefault();

                // If Status is ok, get the session key
                if (lfmStatus != null && lfmStatus == "ok")
                {
                    sessionKey = (from t in resultXml.Element("lfm").Element("session").Elements("key")
                                  select t.Value).FirstOrDefault();
                }
            }

            return sessionKey;
        }

        /// <summary>
        /// Scrobbles a single track
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="track"></param>
        /// <param name="timestamp"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static async Task<bool> TrackScrobble(string sessionKey, string artist, string trackTitle, string albumTitle, DateTime playbackStartTime)
        {
            bool isSuccess = false;

            string method = "track.scrobble";

            var data = new NameValueCollection();

            data["artist"] = artist;
            data["track"] = trackTitle;
            if (!string.IsNullOrEmpty(albumTitle)) data["album"] = albumTitle;
            data["timestamp"] = DateTimeUtils.ConvertToUnixTime(playbackStartTime).ToString();
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequestAsync(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/track.scrobble
                var resultXml = XDocument.Parse(result);

                // Status
                string lfmStatus = (from t in resultXml.Elements("lfm")
                                    select t.Attribute("status").Value).FirstOrDefault();

                // If Status is ok, return true.
                if (lfmStatus != null && lfmStatus == "ok") isSuccess = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Scrobbles a single track
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="track"></param>
        /// <param name="timestamp"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static async Task<bool> TrackUpdateNowPlaying(string sessionKey, string artist, string trackTitle, string albumTitle)
        {
            bool isSuccess = false;

            string method = "track.updateNowPlaying";

            var data = new NameValueCollection();

            data["artist"] = artist;
            data["track"] = trackTitle;
            if (!string.IsNullOrEmpty(albumTitle)) data["album"] = albumTitle;
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequestAsync(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/track.updateNowPlaying
                var resultXml = XDocument.Parse(result);

                // Get the status from the xml
                string lfmStatus = (from t in resultXml.Elements("lfm")
                                    select t.Attribute("status").Value).FirstOrDefault();

                // If the status is ok, return true.
                if (lfmStatus != null && lfmStatus == "ok") isSuccess = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Gets artist information
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static async Task<LastFmArtist> ArtistGetInfo(string artist, bool autoCorrect, string languageCode)
        {
            string method = "artist.getInfo";

            var data = new NameValueCollection();

            if (!string.IsNullOrEmpty(languageCode)) data["lang"] = languageCode;
            data["artist"] = artist;
            data["autocorrect"] = autoCorrect ? "1" : "0"; // 1 = transform misspelled artist names into correct artist names, returning the correct version instead. The corrected artist name will be returned in the response.
            data["api_key"] = SensitiveInformation.LastfmApiKey;

            string result = await PerformGetRequestAsync(method, data, false);

            var lfmArtist = new LastFmArtist();

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/artist.getInfo
                var resultXml = XDocument.Parse(result);

                // Name
                lfmArtist.Name = (from t in resultXml.Element("lfm").Element("artist").Elements("name")
                                  select t.Value).FirstOrDefault();

                // Url
                lfmArtist.Url = (from t in resultXml.Element("lfm").Element("artist").Elements("url")
                                 select t.Value).FirstOrDefault();

                // ImageSmall
                lfmArtist.ImageSmall = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                        where t.Attribute("size").Value == "small"
                                        select t.Value).FirstOrDefault();

                // ImageMedium
                lfmArtist.ImageMedium = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                         where t.Attribute("size").Value == "medium"
                                         select t.Value).FirstOrDefault();

                // ImageLarge
                lfmArtist.ImageLarge = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                        where t.Attribute("size").Value == "large"
                                        select t.Value).FirstOrDefault();

                // ImageExtraLarge
                lfmArtist.ImageExtraLarge = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                             where t.Attribute("size").Value == "extralarge"
                                             select t.Value).FirstOrDefault();

                // ImageMega
                lfmArtist.ImageMega = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                       where t.Attribute("size").Value == "mega"
                                       select t.Value).FirstOrDefault();

                // SimilarArtists
                lfmArtist.SimilarArtists = (from t in resultXml.Element("lfm").Element("artist").Element("similar").Elements("artist")
                                            select new LastFmArtist
                                            {
                                                Name = t.Descendants("name").FirstOrDefault().Value,
                                                Url = t.Descendants("url").FirstOrDefault().Value,
                                                ImageSmall = t.Descendants("image").Where((i) => i.Attribute("size").Value == "small").FirstOrDefault().Value,
                                                ImageMedium = t.Descendants("image").Where((i) => i.Attribute("size").Value == "medium").FirstOrDefault().Value,
                                                ImageLarge = t.Descendants("image").Where((i) => i.Attribute("size").Value == "large").FirstOrDefault().Value,
                                                ImageExtraLarge = t.Descendants("image").Where((i) => i.Attribute("size").Value == "extralarge").FirstOrDefault().Value,
                                                ImageMega = t.Descendants("image").Where((i) => i.Attribute("size").Value == "mega").FirstOrDefault().Value
                                            }).ToList();

                // Biography
                lfmArtist.Biography = (from t in resultXml.Element("lfm").Element("artist").Elements("bio")
                                       select new LastFmBiography
                                       {
                                           Published = t.Descendants("published").FirstOrDefault().Value,
                                           Summary = t.Descendants("summary").FirstOrDefault().Value,
                                           Content = t.Descendants("content").FirstOrDefault().Value
                                       }).FirstOrDefault();
            }

            return lfmArtist;
        }

        /// <summary>
        /// Gets album information
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static async Task<LastFmAlbum> AlbumGetInfo(string artist, string album, bool autoCorrect, string languageCode)
        {
            string method = "album.getInfo";

            var data = new NameValueCollection();

            if (!string.IsNullOrEmpty(languageCode)) data["lang"] = languageCode;
            data["artist"] = artist;
            data["album"] = album;
            data["autocorrect"] = autoCorrect ? "1" : "0"; // 1 = transform misspelled artist names into correct artist names, returning the correct version instead. The corrected artist name will be returned in the response.
            data["api_key"] = SensitiveInformation.LastfmApiKey;

            string result = await PerformGetRequestAsync(method, data, false);

            var lfmAlbum = new LastFmAlbum();

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/album.getInfo
                var resultXml = XDocument.Parse(result);

                // Artist
                lfmAlbum.Artist = (from t in resultXml.Element("lfm").Element("album").Elements("artist")
                                   select t.Value).FirstOrDefault();

                // Name
                lfmAlbum.Name = (from t in resultXml.Element("lfm").Element("album").Elements("name")
                                 select t.Value).FirstOrDefault();

                // Url
                lfmAlbum.Url = (from t in resultXml.Element("lfm").Element("album").Elements("url")
                                select t.Value).FirstOrDefault();

                // ImageSmall
                lfmAlbum.ImageSmall = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                       where t.Attribute("size").Value == "small"
                                       select t.Value).FirstOrDefault();

                // ImageMedium
                lfmAlbum.ImageMedium = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                        where t.Attribute("size").Value == "medium"
                                        select t.Value).FirstOrDefault();

                // ImageLarge
                lfmAlbum.ImageLarge = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                       where t.Attribute("size").Value == "large"
                                       select t.Value).FirstOrDefault();

                // ImageExtraLarge
                lfmAlbum.ImageExtraLarge = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                            where t.Attribute("size").Value == "extralarge"
                                            select t.Value).FirstOrDefault();

                // ImageMega
                lfmAlbum.ImageMega = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                      where t.Attribute("size").Value == "mega"
                                      select t.Value).FirstOrDefault();
            }
            return lfmAlbum;
        }

        /// <summary>
        /// Love a track for a user profile
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="artist"></param>
        /// <param name="trackTitle"></param>
        /// <returns></returns>
        public static async Task<bool> TrackLove(string sessionKey, string artist, string trackTitle)
        {
            bool isSuccess = false;

            string method = "track.love";

            var data = new NameValueCollection();

            data["track"] = trackTitle;
            data["artist"] = artist;
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequestAsync(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/track.love
                var resultXml = XDocument.Parse(result);

                // Status
                string lfmStatus = (from t in resultXml.Elements("lfm")
                                    select t.Attribute("status").Value).FirstOrDefault();

                // If Status is ok, return true.
                if (lfmStatus != null && lfmStatus == "ok") isSuccess = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Unlove a track for a user profile
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="artist"></param>
        /// <param name="trackTitle"></param>
        /// <returns></returns>
        public static async Task<bool> TrackUnlove(string sessionKey, string artist, string trackTitle)
        {
            bool isSuccess = false;

            string method = "track.unlove";

            var data = new NameValueCollection();

            data["artist"] = artist;
            data["track"] = trackTitle;
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequestAsync(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/track.unlove
                var resultXml = XDocument.Parse(result);

                // Status
                string lfmStatus = (from t in resultXml.Elements("lfm")
                                    select t.Attribute("status").Value).FirstOrDefault();

                // If Status is ok, return true.
                if (lfmStatus != null && lfmStatus == "ok") isSuccess = true;
            }

            return isSuccess;
        }

        //public static async Task<List<LastFmTrack>> UserGetLovedTracks(string username)
        //{
        //    string method = "user.getLovedTracks";

        //    var data = new NameValueCollection();

        //    data["user"] = username;
        //    data["api_key"] = SensitiveInformation.LastfmApiKey;

        //    string result = await PerformGetRequest(method, data, false);

        //    if (!string.IsNullOrEmpty(result))
        //    {
        //        // TODO parse result
        //    }
        //}
        #endregion
    }
}
