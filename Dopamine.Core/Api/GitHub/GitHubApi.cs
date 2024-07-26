using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dopamine.Core.Api.GitHub
{
    public static class GitHubApi
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        public static async Task<string> GetLatestReleaseAsync(string owner, string repo, bool includePrereleases) {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var releasesResponse = await httpClient.GetStringAsync(url);
            var releases = JArray.Parse(releasesResponse);

            JObject latestRelease = null;

            if (includePrereleases)
            {
                latestRelease = releases.FirstOrDefault(x => (bool)x["prerelease"]) as JObject;
            }
            else
            {
                latestRelease = releases.FirstOrDefault(x => !(bool)x["prerelease"]) as JObject;
            }

            if (latestRelease != null && latestRelease["tag_name"] != null)
            {
                return latestRelease["tag_name"].ToString().Replace("v", "");
            }

            return string.Empty;
        }
    }
}