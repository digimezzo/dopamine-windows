using Dopamine.Core.IO;
using Dopamine.Core.Settings;

namespace Dopamine.Core.Utils
{
    public static class ArtworkUtils
    {
        public static string GetArtworkPath(string artworkID)
        {
            if (!string.IsNullOrEmpty(artworkID))
            {
                return System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder, ApplicationPaths.CoverArtCacheFolder, artworkID + ".jpg");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
