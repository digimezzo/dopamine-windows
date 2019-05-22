using Digimezzo.Foundation.Core.Helpers;
using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Packaging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Services.Update;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Services.Update
{
    [DataContract]
    internal class OnlineVersionResult
    {
        [DataMember]
        internal string status = null;

        [DataMember]
        internal string status_message = null;

        [DataMember]
        internal string data = null;
    }

    public class UpdateService : IUpdateService
    {
        private string apiRootFormat = Constants.HomeLink + "/content/software/updateapi.php?function=getnewversion&application=Dopamine&version={0}&getprerelease={1}";
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
                Uri uri = new Uri(string.Format(
                    apiRootFormat,
                    currentVersion.UnformattedVersion,
                    SettingsClient.Get<bool>("Updates", "CheckForPrereleases") ? "yes" : "no"
                    ));
                string jsonResult = string.Empty;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    var response = await client.GetAsync(uri);
                    jsonResult = await response.Content.ReadAsStringAsync();
                }

                var newOnlineVersionResult = JsonConvert.DeserializeObject<OnlineVersionResult>(jsonResult);

                if (!string.IsNullOrEmpty(newOnlineVersionResult.data))
                {
                    newVersion = this.CreateDummyPackage(new Version(newOnlineVersionResult.data));
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Update check: could not retrieve online version information. Exception: {0}", ex.Message);
            }

            return newVersion;
        }

        private bool IsValidVersion(Version version)
        {
            return !(version.Major == 0 & version.Minor == 0 & version.Build == 0 & version.Revision == 0);
        }

        private bool IsDownloadedPackageAvailable(string package)
        {
            FileInfo fi = new FileInfo(package);

            return fi.Exists && fi.Length > 0;
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
                string downloadLink = Path.Combine(UpdateInformation.AutomaticDownloadLink, Path.GetFileName(packageToDownload));

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
            if (this.IsValidVersion(newOnlineVersion.Version))
            {
                bool automaticDownload = SettingsClient.Get<bool>("Updates", "AutomaticDownload") & !SettingsClient.Get<bool>("Configuration", "IsPortable");

                if (automaticDownload) // Automatic download is enabled
                {
                    // Define the name of the file to which we will download the update
                    string updatePackageExtractedDirectoryFullPath = Path.Combine(this.updatesSubDirectory, newOnlineVersion.Filename);
                    string updatePackageDownloadedFileFullPath = Path.Combine(this.updatesSubDirectory, newOnlineVersion.Filename + newOnlineVersion.UpdateFileExtension);

                    // Check if there is a directory with the name of the update package: 
                    // that means the file was already downloaded and extracted.
                    if (Directory.Exists(updatePackageExtractedDirectoryFullPath))
                    {

                        // The folder exists, that means that the new version was already extracted previously.
                        if (this.canCheck)
                        {
                            // Raise an event that a new version is available for installation.
                            this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                            {
                                UpdatePackage = newOnlineVersion,
                                UpdatePackageLocation = updatePackageExtractedDirectoryFullPath
                            });
                        }
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
                                    if (this.canCheck)
                                    {
                                        this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                                        {
                                            UpdatePackage = newOnlineVersion,
                                            UpdatePackageLocation = updatePackageExtractedDirectoryFullPath
                                        });
                                    }
                                }
                                else
                                {
                                    // Processing the downloaded file failed. Log the failure reason.
                                    LogClient.Error("Update check: could not process downloaded files. User is notified that there is a new version online. Exception: {0}", processResult.GetFirstMessage());

                                    // Raise an event that there is a new version available online.
                                    if (this.canCheck)
                                    {
                                        this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                                        {
                                            UpdatePackage = newOnlineVersion,
                                            UpdatePackageLocation = updatePackageExtractedDirectoryFullPath
                                        });
                                    }
                                }
                            }
                            else
                            {
                                // Downloading failed: log the failure reason.
                                LogClient.Error("Update check: could not download the file. Exception: {0}", downloadResult.GetFirstMessage());

                                // Downloading failed, but we want the user to know that there is a new version.
                                if (this.canCheck)
                                {
                                    this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                                    {
                                        UpdatePackage = newOnlineVersion
                                    });
                                }
                            }
                        }
                        else
                        {
                            OperationResult extractResult = this.ExtractDownloadedPackage(updatePackageDownloadedFileFullPath);

                            if (extractResult.Result)
                            {
                                // Extracting was successful. Raise an event that a new version is available for installation.
                                if (this.canCheck)
                                {
                                    this.NewVersionAvailable(this, new UpdateAvailableEventArgs()
                                    {
                                        UpdatePackage = newOnlineVersion,
                                        UpdatePackageLocation = updatePackageExtractedDirectoryFullPath
                                    });
                                }
                            }
                            else
                            {
                                // Extracting failed: log the failure reason.
                                LogClient.Error("Update check: could not extract the package. Exception: {0}", extractResult.GetFirstMessage());
                            }
                        }
                    }
                }
                else // Automatic download is not enabled
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
            this.checkTimer.Interval = TimeSpan.FromMinutes(SettingsClient.Get<int>("Updates", "CheckIntervalMinutes")).TotalMilliseconds;
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
