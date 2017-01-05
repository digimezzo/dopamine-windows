using Digimezzo.Utilities.Packaging;
using System;

namespace Dopamine.Common.Services.Update
{
    public interface IUpdateService
    {
        void EnableUpdateCheck();
        void DisableUpdateCheck();
        event Action<Package, string> NewDownloadedVersionAvailable;
        event Action<Package> NewOnlineVersionAvailable;
        event Action<Package> NoNewVersionAvailable;
        event EventHandler UpdateCheckDisabled;
    }
}
