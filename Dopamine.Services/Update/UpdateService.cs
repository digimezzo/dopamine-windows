using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Packaging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Dopamine.Core.Api.GitHub;

namespace Dopamine.Services.Update
{
    [DataContract]
    internal class OnlineVersionResult
    {
        [DataMember] internal string status = null;

        [DataMember] internal string status_message = null;

        [DataMember] internal string data = null;
    }

    public class UpdateService : IUpdateService
    {
        private Timer checkTimer = new Timer();
        private string updatesSubDirectory;
        private bool canCheck;
        private WebClient downloadClient;
        private bool isDismissed;

        public event UpdateAvailableEventHandler NewVersionAvailable = delegate { };
        public event EventHandler NoNewVersionAvailable = delegate { };

        public UpdateService()
        {
            this.updatesSubDirectory = Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.UpdatesFolder);
            this.checkTimer.Elapsed += new ElapsedEventHandler(this.CheckTimerElapsedHandler);
        }

        private void CheckTimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            this.checkTimer.Stop();
            this.CheckNow();
        }

        private Package CreateDummyPackage()
        {
            return new Package(ProductInformation.ApplicationName, new Version("0.0.0.0"));
        }

        private Package CreateDummyPackage(Version version)
        {
            return new Package(ProductInformation.ApplicationName, version);
        }

        private async Task<Package> GetNewVersionAsync(Package currentVersion)
        {
            // Create a dummy package. If the version remains 0.0.0.0, no new version was found.
            Package newVersion = this.CreateDummyPackage();

            try
            {
                // Download a new version from Internet
                string newVersionString = await GitHubApi.GetLatestReleaseAsync("digimezzo", "dopamine-windows",
                    SettingsClient.Get<bool>("Updates", "CheckForPrereleases"));

                if (!string.IsNullOrEmpty(newVersionString))
                {
                    newVersion = this.CreateDummyPackage(new Version(newVersionString));
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Update check: could not retrieve online version information. Exception: {0}",
                    ex.Message);
            }

            return newVersion;
        }

        private async void CheckNow()
        {
            if (this.isDismissed)
            {
                return;
            }

            LogClient.Info("Checking for updates");
            this.canCheck = true;

            // Get the current version
            var currentVersion = this.CreateDummyPackage(ProcessExecutable.AssemblyVersion());
            LogClient.Info("Update check: current version = {0}", currentVersion.Version);

            // Get a new version online
            if (!this.canCheck)
            {
                return; // Stop here if the update check was disabled
            }

            Package newOnlineVersion = await this.GetNewVersionAsync(currentVersion);
            LogClient.Info("Update check: new online version = {0}.{1}.{2}.{3}", newOnlineVersion.UnformattedVersion);

            // Check if the online version is valid
            if (newOnlineVersion.Version.CompareTo(currentVersion.Version) > 0)
            {
                // Raise an event that a new version Is available for download
                if (this.canCheck)
                {
                    this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                    {
                        UpdatePackage = newOnlineVersion
                    });
                }
            }
            else
            {
                this.NoNewVersionAvailable(this, new EventArgs());
                LogClient.Info("No new version was found");
            }

            if (SettingsClient.Get<bool>("Updates", "CheckPeriodically"))
            {
                this.EnablePeriodicCheck();
            }
        }

        private void EnablePeriodicCheck()
        {
            LogClient.Info("Enabling periodic update check");
            this.checkTimer.Stop();
            this.checkTimer.Interval = TimeSpan.FromMinutes(SettingsClient.Get<int>("Updates", "CheckIntervalMinutes"))
                .TotalMilliseconds;
            this.checkTimer.Start();
        }

        private void DisablePeriodicCheck()
        {
            LogClient.Info("Disabling periodic update check");
            this.canCheck = false;
            this.checkTimer.Stop();
        }

        public void Reset()
        {
            LogClient.Info("Resetting update check");

            this.DisablePeriodicCheck();

            this.isDismissed = false;

            if (SettingsClient.Get<bool>("Updates", "CheckAtStartup"))
            {
                this.CheckNow();
            }
            else if (SettingsClient.Get<bool>("Updates", "CheckPeriodically"))
            {
                this.EnablePeriodicCheck();
            }
            else
            {
                this.NoNewVersionAvailable(this, new EventArgs());
            }
        }

        public void Dismiss()
        {
            this.isDismissed = true;
            this.NoNewVersionAvailable(this, new EventArgs());
        }
    }
}