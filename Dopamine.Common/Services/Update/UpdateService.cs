using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Packaging;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.IO;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace Dopamine.Common.Services.Update
{
    [DataContract]
    internal class OnlineVersionResult
    {
        [DataMember]
        internal string status;

        [DataMember]
        internal string status_message;

        [DataMember]
        internal string data;
    }

    public class UpdateService : IUpdateService
    {
        private string apiRootFormat = Base.Constants.HomeLink + "/content/software/updateapi.php?function=getnewversion&application=Dopamine&version={0}";
        private string updatesSubDirectory;
        private bool canCheckForUpdates;
        private bool checkingForUpdates;
        private bool automaticDownload;
        private Timer checkNewVersionTimer = new Timer();
        private WebClient downloadClient;

        public UpdateService()
        {
            this.checkNewVersionTimer.Elapsed += new ElapsedEventHandler(this.CheckNewVersionTimerHandler);

            this.updatesSubDirectory = Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.UpdatesFolder);
            this.canCheckForUpdates = false;
        }

        private Package CreateDummyPackage()
        {
            return new Package(ProductInformation.ApplicationName, new Version("0.0.0.0"), Configuration.Debug);
        }

        private Package CreateDummyPackage(Version version)
        {
            return new Package(ProductInformation.ApplicationName, version, Configuration.Debug);
        }

        private bool IsValidVersion(Version version)
        {
            if (version.Major == 0 & version.Minor == 0 & version.Build == 0 & version.Revision == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<Package> GetNewVersionAsync(Package currentVersion)
        {
            // Create a dummy package. If the version remains 0.0.0.0, no new version was found.
            Package newVersion = this.CreateDummyPackage();

            try
            {
                // Download a new version from Internet
                Uri uri = new Uri(string.Format(apiRootFormat, currentVersion.UnformattedVersion));
                string jsonResult = string.Empty;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    var response = await client.GetAsync(uri);
                    jsonResult = await response.Content.ReadAsStringAsync();
                }

                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonResult)))
                {
                    var deserializer = new DataContractJsonSerializer(typeof(OnlineVersionResult));
                    OnlineVersionResult newOnlineVersionResult = (OnlineVersionResult)deserializer.ReadObject(ms);

                    if (!string.IsNullOrEmpty(newOnlineVersionResult.data))
                    {
                        newVersion = this.CreateDummyPackage(new Version(newOnlineVersionResult.data));
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Update check: could not retrieve online version information. Exception: {0}", ex.Message);
            }

            return newVersion;
        }

        private async Task<bool> TryCreateUpdatesSubDirectoryAsync()
        {
            bool createSuccessful = false;

            await Task.Run(() =>
            {
                try
                {
                    // If the Updates subdirectory doesn't exist, create it
                    if (!Directory.Exists(this.updatesSubDirectory))
                    {
                        Directory.CreateDirectory(this.updatesSubDirectory);
                    }

                    createSuccessful = true;
                }
                catch (Exception ex)
                {
                    LogClient.Error("Update check: could not create the Updates subdirectory. Exception: {0}", ex.Message);
                    createSuccessful = false;
                }
            });

            return createSuccessful;
        }

        private void TryCleanUpdatesSubDirectory()
        {
            try
            {
                foreach (System.IO.FileInfo fi in new DirectoryInfo(this.updatesSubDirectory).GetFiles("*.*"))
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Update check: could not delete the file {0}. Exception: {1}", fi.FullName, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Update check: Error while cleaning the Updates subdirectory. Exception: {0}", ex.Message);
            }
        }

        private async Task<OperationResult> DownloadAsync(Package latestOnlineVersion, string packageToDownload)
        {
            var operationResult = new OperationResult();

            // Try to create the Updates subdirectory. If this fails: return.
            if (!await this.TryCreateUpdatesSubDirectoryAsync())
            {
                operationResult.Result = false;
                return operationResult;
            }

            // Delete all files from the Updates subdirectory. If this fails, just continue.
            this.TryCleanUpdatesSubDirectory();

            try
            {
                string downloadLink = Path.Combine(UpdateInformation.ReleaseAutomaticDownloadLink, Path.GetFileName(packageToDownload));

                if (latestOnlineVersion.Configuration == Configuration.Debug)
                {
                    downloadLink = Path.Combine(UpdateInformation.PreReleaseAutomaticDownloadLink, Path.GetFileName(packageToDownload));
                }

                this.downloadClient = new WebClient();

                LogClient.Info("Update check: downloading file '{0}'", downloadLink);

                await this.downloadClient.DownloadFileTaskAsync(new Uri(downloadLink), packageToDownload + ".part");

                operationResult.Result = true;
            }
            catch (Exception ex)
            {
                operationResult.Result = false;
                operationResult.AddMessage(ex.Message);
            }

            return operationResult;
        }

        private async Task<OperationResult> ProcessDownloadedPackageAsync()
        {
            var operationResult = new OperationResult();

            await Task.Run(() =>
            {
                // Gets the files with extension ".part"
                FileInfo[] partFiles = new DirectoryInfo(this.updatesSubDirectory).GetFiles("*.part");

                // Get the most recent file with extension ".part"
                FileInfo lastPartFile = partFiles.OrderByDescending(pf => pf.CreationTime).FirstOrDefault();
                string lastPackageFile = lastPartFile.FullName.Replace(".part", "");

                // Remove the ".part" extension
                if (lastPartFile != null)
                {
                    System.IO.File.Move(lastPartFile.FullName, lastPackageFile);
                }

                // Extract
                operationResult = this.ExtractDownloadedPackage(lastPackageFile);
            });

            return operationResult;
        }

        private bool IsDownloadedPackageAvailable(string package)
        {
            FileInfo fi = new FileInfo(package);

            if (fi.Exists && fi.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private OperationResult ExtractDownloadedPackage(string package)
        {
            var operationResult = new OperationResult();
            FileInfo fi = new FileInfo(package);
            string destinationPath = Path.Combine(this.updatesSubDirectory, Path.GetFileNameWithoutExtension(package));

            try
            {
                // Extract
                using (ZipFile zip = ZipFile.Read(fi.FullName))
                {
                    zip.ExtractAll(destinationPath, ExtractExistingFileAction.OverwriteSilently);
                }

                operationResult.Result = true;
            }
            catch (Exception ex)
            {
                operationResult.Result = false;
                operationResult.AddMessage(ex.Message);
            }

            return operationResult;
        }

        private async Task CheckForUpdatesAsync()
        {
            // Indicate for the rest of the class that we are checking for updates
            // -------------------------------------------------------------------
            this.checkingForUpdates = true;

            // We start checking for updates: stop the timer
            // ---------------------------------------------
            this.checkNewVersionTimer.Stop();

            // Get the current version
            // -----------------------
            var currentVersion = this.CreateDummyPackage(ProcessExecutable.AssemblyVersion());
            LogClient.Info("Update check: current version = {0}", currentVersion.Version.ToString());

            // Get a new version online
            // ------------------------
            if (!this.canCheckForUpdates)
            {
                return; // Stop here if the update check was disabled while we were running
            }

            Package newOnlineVersion = await this.GetNewVersionAsync(currentVersion);
            LogClient.Info("Update check: new online version = {0}.{1}.{2}.{3}", newOnlineVersion.UnformattedVersion);

            // Check if the online version is valid
            // ------------------------------------
            if (this.IsValidVersion(newOnlineVersion.Version))
            {
                if (this.automaticDownload)
                {
                    // Automatic download is enabled
                    // -----------------------------

                    // Define the name of the file to which we will download the update
                    string updatePackageExtractedDirectoryFullPath = Path.Combine(this.updatesSubDirectory, newOnlineVersion.Filename);
                    string updatePackageDownloadedFileFullPath = Path.Combine(this.updatesSubDirectory, newOnlineVersion.Filename + newOnlineVersion.UpdateFileExtension);

                    // Check if there is a directory with the name of the update package: 
                    // that means the file was already downloaded and extracted.
                    if (Directory.Exists(updatePackageExtractedDirectoryFullPath))
                    {

                        // The folder exists, that means that the new version was already extracted previously.
                        // Raise an event that a new version is available for installation.
                        if (this.canCheckForUpdates) this.NewDownloadedVersionAvailable(newOnlineVersion, updatePackageExtractedDirectoryFullPath);
                    }
                    else
                    {
                        // Check if there is a package with the name of the update package: that would mean the update was already downloaded.
                        if (!this.IsDownloadedPackageAvailable(updatePackageDownloadedFileFullPath))
                        {
                            // No package available yet: download it.
                            OperationResult downloadResult = await this.DownloadAsync(newOnlineVersion, updatePackageDownloadedFileFullPath);

                            if (downloadResult.Result)
                            {
                                OperationResult processResult = await this.ProcessDownloadedPackageAsync();

                                if (processResult.Result)
                                {
                                    // Processing the downloaded file was successful. Raise an event that a new version is available for installation.
                                    if (this.canCheckForUpdates) this.NewDownloadedVersionAvailable(newOnlineVersion, updatePackageExtractedDirectoryFullPath);
                                }
                                else
                                {
                                    // Processing the downloaded file failed. Log the failure reason.
                                    LogClient.Error("Update check: could not process downloaded files. User is notified that there is a new version online. Exception: {0}", processResult.GetFirstMessage());

                                    // Raise an event that there is a new version available online.
                                    if (this.canCheckForUpdates) this.NewOnlineVersionAvailable(newOnlineVersion);
                                }
                            }
                            else
                            {
                                // Downloading failed: log the failure reason.
                                LogClient.Error("Update check: could not download the file. Exception: {0}", downloadResult.GetFirstMessage());
                            }
                        }
                        else
                        {
                            OperationResult extractResult = this.ExtractDownloadedPackage(updatePackageDownloadedFileFullPath);

                            if (extractResult.Result)
                            {
                                // Extracting was successful. Raise an event that a new version is available for installation.
                                if (this.canCheckForUpdates) this.NewDownloadedVersionAvailable(newOnlineVersion, updatePackageExtractedDirectoryFullPath);
                            }
                            else
                            {
                                // Extracting failed: log the failure reason.
                                LogClient.Error("Update check: could not extract the package. Exception: {0}", extractResult.GetFirstMessage());
                            }
                        }
                    }
                }
                else
                {
                    // Automatic download is not enabled
                    // ---------------------------------

                    // Raise an event that a New version Is available for download
                    if (this.canCheckForUpdates) this.NewOnlineVersionAvailable(newOnlineVersion);
                }
            }
            else
            {
                this.NoNewVersionAvailable(newOnlineVersion);
                LogClient.Info("Update check: no newer version was found.");
            }

            // Indicate for the rest of the class that we have finished checking for updates
            // -----------------------------------------------------------------------------
            this.checkingForUpdates = false;

            // We're finished checking for updates: start the timer
            // ----------------------------------------------------
            this.checkNewVersionTimer.Start();
        }

        public void EnableUpdateCheck()
        {
            // Log that we start checking for updates
            LogClient.Info("Update check: checking for updates.");

            // We can check for updates
            this.canCheckForUpdates = true;

            // Set the timer interval based on update settings
            this.checkNewVersionTimer.Interval = TimeSpan.FromSeconds(Base.Constants.UpdateCheckIntervalSeconds).TotalMilliseconds;

            // Set flags based on update settings
            this.automaticDownload = SettingsClient.Get<bool>("Updates", "AutomaticDownload") & !SettingsClient.Get<bool>("Configuration", "IsPortable");

            // Actual update check. Don't await, just run async. (Stops the timer when starting and starts the timer again when ready)
            if (!this.checkingForUpdates) this.CheckForUpdatesAsync();
        }

        public void DisableUpdateCheck()
        {
            this.canCheckForUpdates = false;
            this.checkNewVersionTimer.Stop();

            this.UpdateCheckDisabled(this, null);
        }

        public event Action<Package, string> NewDownloadedVersionAvailable = delegate { };
        public event Action<Package> NewOnlineVersionAvailable = delegate { };
        public event Action<Package> NoNewVersionAvailable = delegate { };
        public event EventHandler UpdateCheckDisabled = delegate { };

        public void CheckNewVersionTimerHandler(object sender, ElapsedEventArgs e)
        {
            // Actual update check. Don't await, just run async. (Stops the timer when starting and starts the timer again when ready)
            if (!this.checkingForUpdates) this.CheckForUpdatesAsync();
        }
    }
}
