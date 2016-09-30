using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Settings;
using System;
using System.IO;

namespace Dopamine.Common.Services.Cache
{
    public class CacheService : ICacheService
    {
        #region Variables
        private string coverArtCacheFolderPath;
        #endregion

        #region Properties
        public string CoverArtCacheFolderPath
        {
            get
            {
                return this.coverArtCacheFolderPath;
            }
        }
        #endregion

        #region Construction
        public CacheService()
        {
            string cacheFolderPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder);
            this.coverArtCacheFolderPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder, ApplicationPaths.CoverArtCacheFolder);

            // If it doesn't exist, create the cache folder.
            if (!Directory.Exists(cacheFolderPath)) Directory.CreateDirectory(cacheFolderPath);

            // If it doesn't exist, create the coverArt cache folder.
            if (!Directory.Exists(this.coverArtCacheFolderPath)) Directory.CreateDirectory(this.coverArtCacheFolderPath);
        }
        #endregion

        #region ICacheService
        public string CacheArtwork(byte[] artwork)
        {
            if (artwork == null) return string.Empty;

            string artworkID = "album-" + Guid.NewGuid().ToString();
            ImageOperations.Byte2Jpg(artwork, Path.Combine(this.coverArtCacheFolderPath, artworkID + ".jpg"), 0, 0, Constants.CoverQualityPercent);

            return artworkID;
        }

        public string GetCachedArtworkPath(string artworkID)
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
        #endregion
    }
}
