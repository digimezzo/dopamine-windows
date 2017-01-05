using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.Update;
using Dopamine.Common.Base;
using Digimezzo.Utilities.Log;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Digimezzo.Utilities.Packaging;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class StatusViewModel : BindableBase
    {
        #region Variables
        // Services
        private IIndexingService indexingService;
        private IUpdateService updateService;

        // Indexing
        private bool isIndexing;
        private string indexingProgress;
        private bool isIndexerRemovingSongs;
        private bool isIndexerAddingSongs;
        private bool isIndexerUpdatingSongs;
        private bool isIndexerUpdatingArtwork;

        // Update status
        private bool isUpdateAvailable;
        private Package package;
        private string destinationPath;
        private string updateToolTip;
        private bool isUpdateStatusHiddenByUser;
        #endregion

        #region Properties
        // Status bar
        public bool IsStatusBarVisible
        {
            get { return this.isIndexing | this.isUpdateAvailable; }
        }

        // Indexing
        public bool IsIndexing
        {
            get { return this.isIndexing; }
            set
            {
                SetProperty<bool>(ref this.isIndexing, value);
                OnPropertyChanged(() => this.IsStatusBarVisible);
            }
        }

        public string IndexingProgress
        {
            get { return this.indexingProgress; }
            set { SetProperty<string>(ref this.indexingProgress, value); }
        }

        public bool IsIndexerRemovingSongs
        {
            get { return this.isIndexerRemovingSongs; }
            set { SetProperty<bool>(ref this.isIndexerRemovingSongs, value); }
        }

        public bool IsIndexerAddingSongs
        {
            get { return this.isIndexerAddingSongs; }
            set { SetProperty<bool>(ref this.isIndexerAddingSongs, value); }
        }

        public bool IsIndexerUpdatingSongs
        {
            get { return this.isIndexerUpdatingSongs; }
            set { SetProperty<bool>(ref this.isIndexerUpdatingSongs, value); }
        }

        public bool IsIndexerUpdatingArtwork
        {
            get { return this.isIndexerUpdatingArtwork; }
            set { SetProperty<bool>(ref this.isIndexerUpdatingArtwork, value); }
        }

        // Update status
        public bool IsUpdateAvailable
        {
            get { return this.isUpdateAvailable; }
            set
            {
                SetProperty<bool>(ref this.isUpdateAvailable, value);
                OnPropertyChanged(() => this.IsStatusBarVisible);
            }
        }

        public Package Package
        {
            get { return this.package; }
            set { SetProperty<Package>(ref this.package, value); }
        }

        public string UpdateToolTip
        {
            get { return this.updateToolTip; }
            set { SetProperty<string>(ref this.updateToolTip, value); }
        }

        public bool ShowInstallUpdateButton
        {
            get { return !string.IsNullOrEmpty(this.destinationPath); }
        }
        #endregion

        #region Commands
        public DelegateCommand DownloadOrInstallUpdateCommand { get; set; }
        public DelegateCommand HideUpdateStatusCommand { get; set; }
        #endregion

        #region Construction
        public StatusViewModel(IUpdateService updateService, IIndexingService indexingService)
        {
            this.updateService = updateService;
            this.indexingService = indexingService;

            this.DownloadOrInstallUpdateCommand = new DelegateCommand(this.DownloadOrInstallUpdate);
            this.HideUpdateStatusCommand = new DelegateCommand(() =>
            {
                this.isUpdateStatusHiddenByUser = true;
                this.IsUpdateAvailable = false;
            });

            this.indexingService.IndexingStatusChanged += async(indexingStatusEventArgs) => await IndexingService_IndexingStatusChangedAsync(indexingStatusEventArgs);
            this.indexingService.IndexingStopped += IndexingService_IndexingStopped;

            this.updateService.NewDownloadedVersionAvailable += NewVersionAvailableHandler;
            this.updateService.NewOnlineVersionAvailable += NewVersionAvailableHandler;
            this.updateService.NoNewVersionAvailable += NoNewVersionAvailableHandler;
            this.updateService.UpdateCheckDisabled += (_,__) => this.IsUpdateAvailable = false;

            if (SettingsClient.Get<bool>("Updates", "CheckForUpdates"))
            {
                this.updateService.EnableUpdateCheck();
            }

            // Initial status
            this.IsIndexing = false;
            this.IsUpdateAvailable = false;
        }
        #endregion

        #region Event Handlers
        private async Task IndexingService_IndexingStatusChangedAsync(IndexingStatusEventArgs indexingStatusEventArgs)
        {
            await Task.Run(() =>
            {
                this.IsIndexing = this.indexingService.IsIndexing;

                if (this.IsIndexing)
                {
                    this.updateService.DisableUpdateCheck();
                    this.IsUpdateAvailable = false;

                    switch (indexingStatusEventArgs.IndexingAction)
                    {
                        case IndexingAction.RemoveTracks:
                            this.IsIndexerRemovingSongs = true;
                            this.IsIndexerAddingSongs = false;
                            this.IsIndexerUpdatingSongs = false;
                            this.IsIndexerUpdatingArtwork = false;
                            this.IndexingProgress = string.Empty;
                            break;
                        case IndexingAction.AddTracks:
                            this.IsIndexerRemovingSongs = false;
                            this.IsIndexerAddingSongs = true;
                            this.IsIndexerUpdatingSongs = false;
                            this.IsIndexerUpdatingArtwork = false;
                            this.IndexingProgress = this.FillProgress(indexingStatusEventArgs.ProgressCurrent.ToString(), indexingStatusEventArgs.ProgressTotal.ToString());
                            break;
                        case IndexingAction.UpdateTracks:
                            this.IsIndexerRemovingSongs = false;
                            this.IsIndexerAddingSongs = false;
                            this.IsIndexerUpdatingSongs = true;
                            this.IsIndexerUpdatingArtwork = false;
                            this.IndexingProgress = this.FillProgress(indexingStatusEventArgs.ProgressCurrent.ToString(), indexingStatusEventArgs.ProgressTotal.ToString());
                            break;
                        case IndexingAction.UpdateArtwork:
                            this.IsIndexerRemovingSongs = false;
                            this.IsIndexerAddingSongs = false;
                            this.IsIndexerUpdatingSongs = false;
                            this.IsIndexerUpdatingArtwork = true;
                            this.IndexingProgress = string.Empty;
                            break;
                        default:
                            break;
                            // Never happens
                    }
                }
                else
                {
                    this.IndexingProgress = string.Empty;

                    if (SettingsClient.Get<bool>("Updates", "CheckForUpdates"))
                    {
                        this.updateService.EnableUpdateCheck();
                    }
                }
            });
        }

        private string FillProgress(string currentProgres, string totalProgress)
        {
            string progress = string.Empty;

            progress = "(" + ResourceUtils.GetStringResource("Language_Current_Of_Total") + ")";
            progress = progress.Replace("%current%", currentProgres);
            progress = progress.Replace("%total%", totalProgress);

            return progress;
        }

        private void IndexingService_IndexingStopped(object sender, EventArgs e)
        {
            if (this.IsIndexing)
            {
                this.IsIndexing = false;
                this.IndexingProgress = string.Empty;

                if (SettingsClient.Get<bool>("Updates", "CheckForUpdates"))
                {
                    this.updateService.EnableUpdateCheck();
                }
            }
        }
        #endregion

        #region Private
        private void NewVersionAvailableHandler(Package package)
        {
            this.NewVersionAvailableHandler(package, string.Empty);
        }

        private void NewVersionAvailableHandler(Package package, string destinationPath)
        {
            if (!this.isUpdateStatusHiddenByUser && !this.IsIndexing)
            {
                this.Package = package;
                this.IsUpdateAvailable = true;

                this.destinationPath = destinationPath;
                OnPropertyChanged(() => this.ShowInstallUpdateButton);

                if (!string.IsNullOrEmpty(destinationPath))
                {
                    this.UpdateToolTip = ResourceUtils.GetStringResource("Language_Click_Here_To_Install");
                }
                else
                {
                    this.UpdateToolTip = ResourceUtils.GetStringResource("Language_Click_Here_To_Download");
                }
            }
        }

        private void NoNewVersionAvailableHandler(Package package)
        {
            this.IsUpdateAvailable = false;
        }

        private void DownloadOrInstallUpdate()
        {
            if (!string.IsNullOrEmpty(this.destinationPath))
            {
                try
                {
                    // A file was downloaded. Start the installer.
                    System.IO.FileInfo msiFileInfo = new System.IO.DirectoryInfo(this.destinationPath).GetFiles("*" + package.InstallableFileExtension).First();
                    Process.Start(msiFileInfo.FullName);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not start the MSI installer. Download link was opened instead. Exception: {0}", ex.Message);
                    this.OpenDownloadLink();
                }
            }
            else
            {
                // Nothing was downloaded, forward to the download site.
                this.OpenDownloadLink();
            }
        }

        private void OpenDownloadLink()
        {
            try
            {
                string downloadLink = string.Empty;

                if (this.Package.Configuration == Configuration.Debug)
                {
                    downloadLink = UpdateInformation.PreReleaseDownloadLink;
                }
                else if (this.Package.Configuration == Configuration.Release)
                {
                    downloadLink = UpdateInformation.ReleaseDownloadLink;
                }

                Actions.TryOpenLink(downloadLink);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not open the download link. Exception: {0}", ex.Message);
            }
        }
        #endregion

    }
}
