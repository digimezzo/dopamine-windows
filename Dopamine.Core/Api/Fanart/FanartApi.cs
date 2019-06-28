using Dopamine.Core.Base;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Fanart
{
    public static class FanartApi
    {
        private const string apiRootFormat = "http://webservice.fanart.tv/v3/music/{0}?api_key={1}";

        public async static Task<string> GetArtistThumbnailAsync(string musicBrainzId)
        {
            string jsonResult = await GetArtistImages(musicBrainzId);
            dynamic dynamicObject = JsonConvert.DeserializeObject(jsonResult);

            return dynamicObject.artistthumb[0].url;
        }

        private async static Task<string> GetArtistImages(string musicBrainzId)
        {
            string result = string.Empty;

            Uri uri = new Uri(string.Format(apiRootFormat, musicBrainzId, SensitiveInformation.FanartApiKey));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ExpectContinue = false;
                var response = await client.GetAsync(uri);
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
