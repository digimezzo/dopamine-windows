using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lastfm
{
    public static class AuthenticationApi
    {
        /// <summary>
        /// Requests authorization from the user by sending a POST request over HTTPS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Session key</returns>
        public static async Task<string> GetMobileSession(string username, string password)
        {
            string method = "auth.getMobileSession";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            parameters.Add("username", username);
            parameters.Add("password", password);
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);

            string apiSig = GenerateMethodSignature(parameters, method);
            parameters.Add("api_sig", apiSig);

            string result = await WebApi.PerformPostRequestAsync(method, parameters, true);

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
        /// Constructs an API method signature as described in point 4 on http://www.last.fm/api/mobileauth
        /// </summary>
        /// <param name="data"></param>
        /// <param name="method"></param>
        /// <returns>API method signature</returns>
        public static string GenerateMethodSignature(IEnumerable<KeyValuePair<string, string>> parameters, string method)
        {
            var alphabeticalList = new List<string>();

            // Add everything to the list
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                alphabeticalList.Add(string.Format("{0}{1}", parameter.Key, parameter.Value));
            }

            alphabeticalList.Add("method" + method);

            // Order the list alphabetically
            alphabeticalList = alphabeticalList.OrderBy((t) => t).ToList();


            // Join all parts of the list alphabetically and append API secret
            string signature = string.Format("{0}{1}", string.Join("", alphabeticalList.ToArray()), SensitiveInformation.LastfmSharedSecret);

            // Create MD5 hash and return that
            return CryptographyUtils.MD5Hash(signature);
        }
    }
}
