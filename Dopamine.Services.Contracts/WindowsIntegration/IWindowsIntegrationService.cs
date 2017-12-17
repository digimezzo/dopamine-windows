using System;

namespace Dopamine.Services.Contracts.WindowsIntegration
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
