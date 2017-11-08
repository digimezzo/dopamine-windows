using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Update
{
    public delegate void UpdateAvailableEventHandler(object sender, UpdateAvailableEventArgs e);

    public interface IUpdateService
    {
        void Reset();
        event UpdateAvailableEventHandler NewVersionAvailable;
        event EventHandler NoNewVersionAvailable;
    }
}
