using System;

namespace Dopamine.Services.WindowsIntegration
{
    public interface IWindowsIntegrationService
    {
        bool IsTabletModeEnabled { get; }
        bool IsStartedFromExplorer { get; }
        event EventHandler TabletModeChanged;
        void StartMonitoringTabletMode();
        void StopMonitoringTabletMode();
    }
}
