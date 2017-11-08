using Digimezzo.Utilities.Packaging;
using System;

namespace Dopamine.Common.Services.Update
{
    public delegate void UpdateAvailableEventHandler(object sender, UpdateAvailableEventArgs e);

    public interface IUpdateService
    {
        void EnableUpdateCheck();
        void DisableUpdateCheck();
        event UpdateAvailableEventHandler NewVersionAvailable;
        event EventHandler NoNewVersionAvailable;
    }
}
