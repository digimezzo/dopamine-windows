using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Cache
{
    public class CacheService : ICacheService
    {
        #region Variables
        private string coverArtCacheFolderPath;
        private string temporaryCacheFolderPath;
        #endregion

        #region Properties
        public string CoverArtCacheFolderPath
        {
            get
            {
                return this.coverArtCacheFolderPath;
            }
        }

        public string TemporaryCacheFolderPath
        {
            get
            {
                return this.temporaryCacheFolderPath;
            }
        }
        #endregion

        #region Construction
        public CacheService()
        {
            string cacheFolderPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder);
            this.coverArtCacheFolderPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder, ApplicationPaths.CoverArtCacheFolder);
            this.temporaryCacheFolderPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheFolder, ApplicationPaths.TemporaryCacheFolder);

            // If it doesn't exist, create the cache folder.
            if (!Directory.Exists(cacheFolderPath)) Directory.CreateDirectory(cacheFolderPath);

            // If it doesn't exist, create the coverArt cache folder.
            if (!Directory.Exists(this.coverArtCacheFolderPath)) Directory.CreateDirectory(this.coverArtCacheFolderPath);

            // If it exists, delete the temporary cache folder and create it again (this makes sure it is cleaned from time to time)
            if (Directory.Exists(this.temporaryCacheFolderPath))
            {
                try
                {
                    Directory.Delete(this.temporaryCacheFolderPath, true);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not delete the temporary cache folder. Exception: {0}", ex.Message);
                }
            }

            // If the temporary cache folder doesn't exist, create it.
            if (!Directory.Exists(this.temporaryCacheFolderPath)) Directory.CreateDirectory(this.temporaryCacheFolderPath);
        }
        #endregion

        #region ICacheService
        public async Task<string> CacheArtworkAsync(byte[] artwork)
        {
            if (artwork == null) return string.Empty;

            string artworkID = "album-" + Guid.NewGuid().ToString();

            try
            {
                await Task.Run(() =>
                {
                    ImageOperations.Byte2Jpg(artwork, Path.Combine(this.coverArtCacheFolderPath, artworkID + ".jpg"), 0, 0, Constants.CoverQualityPercent);
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could convert artwork byte[]to JPG. Exception: {0}", ex.Message);
                artworkID = string.Empty;
            }

            return artworkID;
        }

        public string GetCachedArtworkPath(string artworkID)
        {
            if (!string.IsNullOrEmpty(artworkID))
            {
                return System.IO.Path.Combine(coverArtCacheFolderPath, artworkID + ".jpg");
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<string> CacheOnlineFileAsync(Uri uri)
        {
            string cachedFilePath = string.Empty;



            return cachedFilePath;
        }
        #endregion
    }
}
