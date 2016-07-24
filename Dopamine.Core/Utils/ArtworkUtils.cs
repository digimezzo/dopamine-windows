using Dopamine.Core.Database.Entities;
using Dopamine.Core.IO;
using Dopamine.Core.Settings;

namespace Dopamine.Core.Utils
{
    public static class ArtworkUtils
    {
        public static string GetArtworkPath(Album album)
        {
            if (!string.IsNullOrEmpty(album.ArtworkID))
            {
                return System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheSubDirectory, ApplicationPaths.CoverArtCacheSubDirectory, album.ArtworkID + ".jpg");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
