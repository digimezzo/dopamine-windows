using System;

namespace Dopamine.Services.WindowsIntegration
{
    public interface IWindowsIntegrationService
    {
        bool IsTabletModeEnabled { get; }
        bool IsSystemUsingLightTheme { get; }
        bool IsStartedFromExplorer { get; }
        event EventHandler TabletModeChanged;
        event EventHandler SystemUsesLightThemeChanged;
        void StartMonitoringTabletMode();
        void StopMonitoringTabletMode();
        void StartMonitoringSystemUsesLightTheme();
        void StopMonitoringSystemUsesLightTheme();
    }
}
