using Dopamine.Common.Logging;
using Microsoft.Win32;
using System;
using System.Management;
using System.Security.Principal;

namespace Dopamine.Common.Services.WindowsIntegration
{
    public class WindowsIntegrationService : IWindowsIntegrationService
    {
        #region Private
        private ManagementEventWatcher managementEventWatcher;
        #endregion

        #region IWindowsIntegrationService
        public event EventHandler TabletModeChanged = delegate { };

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
                    CoreLogger.Current.Error("Could not get tablet mode from registry. Exception: {0}", ex.Message);
                }

                return registryTabletMode == 1 ? true : false;
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
                    this.managementEventWatcher = new ManagementEventWatcher(wqlEventQuery);
                    this.managementEventWatcher.EventArrived += this.ManagementEventWatcher_EventArrived;
                    this.managementEventWatcher.Start();
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Could not start monitoring tablet mode. Exception: {0}", ex.Message);
            }
        }

        public void StopMonitoringTabletMode()
        {
            try
            {
                if (this.managementEventWatcher != null)
                {
                    this.managementEventWatcher.Stop();
                    this.managementEventWatcher.EventArrived -= this.ManagementEventWatcher_EventArrived;
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("Could not stop monitoring tablet mode. Exception: {0}", ex.Message);
            }
        }

        private void ManagementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            this.TabletModeChanged(this, new EventArgs());
        }
        #endregion
    }
}
