#if WINDOWS_UWP
      using Windows.Storage;
#else
using Digimezzo.Utilities.Settings;
#endif

namespace Dopamine.Core.IO
{
    public sealed class Storage
    {
        public static string StorageFolder
        {
            get
            {
#if WINDOWS_UWP
                return ApplicationData.Current.LocalFolder.Path;
#else
                return SettingsClient.ApplicationFolder();
#endif
            }
        }

    }
}
