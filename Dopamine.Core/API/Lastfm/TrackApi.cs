using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lastfm
{
    public static class TrackApi
    {
        /// <summary>
        /// Scrobbles a single track
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="track"></param>
        /// <param name="timestamp"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static async Task<bool> Scrobble(string sessionKey, string artist, string trackTitle, string albumTitle, DateTime playbackStartTime)
        {
            bool isSuccess = false;

            string method = "track.scrobble";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            parameters.Add("artist", artist);
            parameters.Add("track", trackTitle);
            if (!string.IsNullOrEmpty(albumTitle)) parameters.Add("album", albumTitle);
            parameters.Add("timestamp", DateTimeUtils.ConvertToUnixTime(playbackStartTime).ToString());
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);
            parameters.Add("sk", sessionKey);

            string apiSig = AuthenticationApi.GenerateMethodSignature(parameters, method);
            parameters.Add("api_sig", apiSig);

            string result = await WebApi.PerformPostRequestAsync(method, parameters, false);

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
        public static async Task<bool> UpdateNowPlaying(string sessionKey, string artist, string trackTitle, string albumTitle)
        {
            bool isSuccess = false;

            string method = "track.updateNowPlaying";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            parameters.Add("artist", artist);
            parameters.Add("track", trackTitle);
            if (!string.IsNullOrEmpty(albumTitle)) parameters.Add("album", albumTitle);
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);
            parameters.Add("sk", sessionKey);

            string apiSig = AuthenticationApi.GenerateMethodSignature(parameters, method);
            parameters.Add("api_sig", apiSig);

            string result = await WebApi.PerformPostRequestAsync(method, parameters, false);

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
        /// Love a track for a user profile
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="artist"></param>
        /// <param name="trackTitle"></param>
        /// <returns></returns>
        public static async Task<bool> Love(string sessionKey, string artist, string trackTitle)
        {
            bool isSuccess = false;

            string method = "track.love";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            parameters.Add("track", trackTitle);
            parameters.Add("artist", artist);
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);
            parameters.Add("sk", sessionKey);

            string apiSig = AuthenticationApi.GenerateMethodSignature(parameters, method);
            parameters.Add("api_sig", apiSig);

            string result = await WebApi.PerformPostRequestAsync(method, parameters, false);

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
        public static async Task<bool> Unlove(string sessionKey, string artist, string trackTitle)
        {
            bool isSuccess = false;

            string method = "track.unlove";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            parameters.Add("artist", artist);
            parameters.Add("track", trackTitle);
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);
            parameters.Add("sk", sessionKey);

            string apiSig = AuthenticationApi.GenerateMethodSignature(parameters, method);
            parameters.Add("api_sig", apiSig);

            string result = await WebApi.PerformPostRequestAsync(method, parameters, false);

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
    }
}
