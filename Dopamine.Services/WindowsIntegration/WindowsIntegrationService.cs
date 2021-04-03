using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Services.Playback;
using Microsoft.Win32;
using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Dopamine.Services.WindowsIntegration
{
    public class WindowsIntegrationService : IWindowsIntegrationService
    {
        private IPlaybackService playbackService;
        private ManagementEventWatcher tabletModeWatcher;
        private ManagementEventWatcher systemUsesLightThemeWatcher;
        private bool isStartedFromExplorer;
    
        public WindowsIntegrationService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.isStartedFromExplorer = Environment.GetCommandLineArgs().Length > 1;

            if(SettingsClient.Get<bool>("Playback", "PreventSleepWhilePlaying"))
            {
                this.EnableSleepPrevention();
            }

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Playback", "PreventSleepWhilePlaying"))
                {
                    bool preventSleepWhilePlaying = (bool)e.Entry.Value;

                    if (preventSleepWhilePlaying)
                    {
                        this.EnableSleepPrevention();
                    }
                    else
                    {
                        this.DisableSleepPrevention();
                    }
                }
            };
        }

        private void EnableSleepPrevention()
        {
            this.playbackService.PlaybackFailed += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackPaused += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackStopped += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackResumed += (_, __) => this.DisableSleep();
            this.playbackService.PlaybackSuccess += (_, __) => this.DisableSleep();

            if (this.playbackService.IsPlaying)
            {
                this.DisableSleep();
            }
        }

        private void DisableSleepPrevention()
        {
            this.playbackService.PlaybackFailed -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackPaused -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackStopped -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackResumed -= (_, __) => this.DisableSleep();
            this.playbackService.PlaybackSuccess -= (_, __) => this.DisableSleep();

            this.RestoreSleep();
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public event EventHandler TabletModeChanged = delegate { };
        public event EventHandler SystemUsesLightThemeChanged = delegate { };

        public bool IsTabletModeEnabled
        {
            get
            {
                int registryTabletMode = 0;

                try
                {
                    registryTabletMode = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell", "TabletMode", 0);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get tablet mode from registry. Exception: {0}", ex.Message);
                }

                return registryTabletMode == 1 ? true : false;
            }
        }

        public bool IsSystemUsingLightTheme
        {
            get
            {
                int registrySystemUsesLightTheme = 0;

                try
                {
                    registrySystemUsesLightTheme = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", 0);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get system uses light theme from registry. Exception: {0}", ex.Message);
                }

                return registrySystemUsesLightTheme == 1 ? true : false;
            }
        }

        public bool IsStartedFromExplorer
        {
            get
            {
                // Only returns true the first time we ask for the property
                bool returnValue = this.isStartedFromExplorer;
                this.isStartedFromExplorer = false;
                return returnValue;
            }
        }

        public void StartMonitoringTabletMode()
        {
            try
            {
                var currentUser = WindowsIdentity.GetCurrent();
                if (currentUser != null && currentUser.User != null)
                {
                    var wqlEventQuery = new EventQuery(string.Format(@"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{0}\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell' AND ValueName='TabletMode'", currentUser.User.Value));
                    this.tabletModeWatcher = new ManagementEventWatcher(wqlEventQuery);
                    this.tabletModeWatcher.EventArrived += this.TabletModeWatcher_EventArrived;
                    this.tabletModeWatcher.Start();
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not start monitoring tablet mode. Exception: {0}", ex.Message);
            }
        }

        public void StopMonitoringTabletMode()
        {
            try
            {
                if (this.tabletModeWatcher != null)
                {
                    this.tabletModeWatcher.Stop();
                    this.tabletModeWatcher.EventArrived -= this.TabletModeWatcher_EventArrived;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not stop monitoring tablet mode. Exception: {0}", ex.Message);
            }
        }

        public void StartMonitoringSystemUsesLightTheme()
        {
            try
            {
                var currentUser = WindowsIdentity.GetCurrent();
                if (currentUser != null && currentUser.User != null)
                {
                    var wqlEventQuery = new EventQuery(string.Format(@"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{0}\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize' AND ValueName='SystemUsesLightTheme'", currentUser.User.Value));
                    this.systemUsesLightThemeWatcher = new ManagementEventWatcher(wqlEventQuery);
                    this.systemUsesLightThemeWatcher.EventArrived += this.AppsUseLightThemeWatcher_EventArrived;
                    this.systemUsesLightThemeWatcher.Start();
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not start monitoring system uses light theme. Exception: {0}", ex.Message);
            }
        }

        public void StopMonitoringSystemUsesLightTheme()
        {
            try
            {
                if (this.systemUsesLightThemeWatcher != null)
                {
                    this.systemUsesLightThemeWatcher.Stop();
                    this.systemUsesLightThemeWatcher.EventArrived -= this.AppsUseLightThemeWatcher_EventArrived;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not stop monitoring system uses light theme. Exception: {0}", ex.Message);
            }
        }

        private void TabletModeWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            this.TabletModeChanged(this, new EventArgs());
        }

        private void AppsUseLightThemeWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            this.SystemUsesLightThemeChanged(this, new EventArgs());
        }

        private void DisableSleep()
        {
            try
            {
                // To stop screen saver and monitor power off event
                // You can combine several flags and specify multiple behaviors with a single call
                // See: https://stackoverflow.com/questions/38282770/stop-screensaver-programmatically
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception ex)
            {
                LogClient.Error("Failed to disable sleep. Exception: {0}", ex.Message);
            }
           
        }

        private void RestoreSleep()
        {
            try
            {
                // To reset or allow those events again you have to call this API with only ES_CONTINUOUS
                // See: https://stackoverflow.com/questions/38282770/stop-screensaver-programmatically
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception ex)
            {
                LogClient.Error("Failed to restore sleep. Exception: {0}", ex.Message);
            }
        }
    }
}
