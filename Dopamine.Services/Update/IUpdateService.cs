using System;
using System.Threading.Tasks;

namespace Dopamine.Services.Update
{
    public delegate void UpdateAvailableEventHandler(object sender, UpdateAvailableEventArgs e);

    public interface IUpdateService
    {
        Task Reset();
        void Dismiss();
        event UpdateAvailableEventHandler NewVersionAvailable;
        event EventHandler NoNewVersionAvailable;
    }
}
