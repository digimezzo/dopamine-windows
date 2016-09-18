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
        private static async Task<string> PerformPostRequest(string method, NameValueCollection data, bool useHttps)
        {
            string prefix = "http";
            if (useHttps) prefix = "https";

            string result = string.Empty;
            string url = Uri.EscapeUriString(string.Format("{0}://ws.audioscrobbler.com/2.0/?method={1}", prefix, method));

            using (var client = new WebClient())
            {
                byte[] responseBytes = await client.UploadValuesTaskAsync(url, "POST", data); // UploadValues performs a POST method request
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

            string result = await PerformPostRequest(method, data, true);

            // If the status of the result is ok, get the session key.
            string sessionKey = string.Empty;

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    var resultXml = XDocument.Parse(result);

                    // Get the status from the xml
                    string lfmStatus = (from t in resultXml.Elements("lfm")
                                        select t.Attribute("status").Value).FirstOrDefault();

                    // If the status is ok, get the session key
                    if (lfmStatus != null && lfmStatus == "ok")
                    {
                        sessionKey = (from t in resultXml.Element("lfm").Element("session").Elements("key")
                                      select t.Value).FirstOrDefault();
                    }
                }
                catch (Exception)
                {
                    // Swallow: check for sessionKey.IsNullOrEmpty
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
        public static async Task<bool> TrackScrobble(string username, string password, string sessionKey, string artist, string trackTitle, DateTime playbackStartTime)
        {
            bool isScrobbleSuccess = false;

            string method = "track.scrobble";

            var data = new NameValueCollection();

            data["artist"] = artist;
            data["track"] = trackTitle;
            data["timestamp"] = DateTimeUtils.ConvertToUnixTime(playbackStartTime).ToString();
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequest(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    var resultXml = XDocument.Parse(result);

                    // Get the status from the xml
                    string lfmStatus = (from t in resultXml.Elements("lfm")
                                        select t.Attribute("status").Value).FirstOrDefault();

                    // If the status is ok, return true.
                    if (lfmStatus != null && lfmStatus == "ok") isScrobbleSuccess = true;
                }
                catch (Exception)
                {
                    // Swallow: check for isScrobbleSuccess = true
                }
            }

            return isScrobbleSuccess;
        }

        /// <summary>
        /// Scrobbles a single track
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="track"></param>
        /// <param name="timestamp"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static async Task<bool> TrackUpdateNowPlaying(string username, string password, string sessionKey, string artist, string trackTitle)
        {
            bool isUpdateNowPlayingSuccess = false;

            string method = "track.updateNowPlaying";

            var data = new NameValueCollection();

            data["artist"] = artist;
            data["track"] = trackTitle;
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["sk"] = sessionKey;

            data["api_sig"] = GenerateSignature(data, method);

            string result = await PerformPostRequest(method, data, false);

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    var resultXml = XDocument.Parse(result);

                    // Get the status from the xml
                    string lfmStatus = (from t in resultXml.Elements("lfm")
                                        select t.Attribute("status").Value).FirstOrDefault();

                    // If the status is ok, return true.
                    if (lfmStatus != null && lfmStatus == "ok") isUpdateNowPlayingSuccess = true;
                }
                catch (Exception)
                {
                    // Swallow: check for isUpdateNowPlayingSuccess = true
                }
            }

            return isUpdateNowPlayingSuccess;
        }
        #endregion
    }
}
