using Dopamine.Core.Base;
using System;

namespace Dopamine.Common.Services.Update
{
    public interface IUpdateService
    {
        void EnableUpdateCheck();
        void DisableUpdateCheck();
        event Action<VersionInfo, string> NewDownloadedVersionAvailable;
        event Action<VersionInfo> NewOnlineVersionAvailable;
        event Action<VersionInfo> NoNewVersionAvailable;
        event EventHandler UpdateCheckDisabled;
    }
}
