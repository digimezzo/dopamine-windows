using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lastfm
{
    public static class WebApi
    {
        /// <summary>
        /// Performs a POST request over HTTP or HTTPS
        /// </summary>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="useHttps"></param>
        /// <returns></returns>
        public static async Task<string> PerformPostRequestAsync(string method, IEnumerable<KeyValuePair<string, string>> parameters, bool isSecure)
        {
            string protocol = isSecure ? "https" : "http";
            string result = string.Empty;
            string url = Uri.EscapeUriString(string.Format(Constants.ApiRootFormat, protocol, method));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.PostAsync(url, new FormUrlEncodedContent(parameters));
                result = await response.Content.ReadAsStringAsync();
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
        public static async Task<string> PerformGetRequestAsync(string method, IEnumerable<KeyValuePair<string, string>> parameters, bool isSecure)
        {
            string protocol = isSecure ? "https" : "http";
            string result = string.Empty;
            var dataList = new List<string>();

            // Add everything to the list
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                dataList.Add(string.Format("{0}={1}", parameter.Key, parameter.Value));
            }

            string url = Uri.EscapeUriString(string.Format(Constants.ApiRootFormat + "&{2}", protocol, method, string.Join("&", dataList.ToArray())));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.GetAsync(url);
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
