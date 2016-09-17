using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.API.Lastfm
{
    public class LastfmAPI
    {
        #region Private
        /// <summary>
        /// Constructs an API method signature as described in point 4 on http://www.last.fm/api/mobileauth
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>API method signature</returns>
        private string GenerateMobileSessionApiSignature(string username, string password)
        {
            string signatureString = string.Format("api_key{0}methodauth.getMobileSessionpassword{1}username{2}{3}", SensitiveInformation.LastfmApiKey, password, username, SensitiveInformation.LastfmSharedSecret);
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
        public async Task<string> GetMobileSession(string username, string password)
        {
            string url = Uri.EscapeUriString("https://ws.audioscrobbler.com/2.0/?method=auth.getMobileSession");

            var data = new NameValueCollection();
            data["username"] = username;
            data["password"] = password;
            data["api_key"] = SensitiveInformation.LastfmApiKey;
            data["api_sig"] = this.GenerateMobileSessionApiSignature(username, password);

            string result = string.Empty;

            using (var client = new WebClient { Proxy = null })
            {
                byte[] responseBytes = await client.UploadValuesTaskAsync(url, "POST", data); // UploadValues performs a POST method request
                result = Encoding.UTF8.GetString(responseBytes);
            }

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
                    // Swallow
                }
            }

            return sessionKey;
        }
        #endregion
    }
}
