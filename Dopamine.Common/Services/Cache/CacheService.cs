using Dopamine.Core.IO;
using Dopamine.Core.Settings;
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
        }
        #endregion
    }
}
