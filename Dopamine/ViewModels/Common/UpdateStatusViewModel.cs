using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Packaging;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Services.Contracts.Update;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Linq;

namespace Dopamine.ViewModels.Common
{
    public class UpdateStatusViewModel : BindableBase
    {
        private IUpdateService updateService;
        private bool isUpdateAvailable;
        private Package package;
        private string destinationPath;
        private string updateToolTip;

        public bool IsUpdateAvailable
        {
            get { return this.isUpdateAvailable; }
            set
            {
                SetProperty<bool>(ref this.isUpdateAvailable, value);
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

        public DelegateCommand DownloadOrInstallUpdateCommand { get; set; }
        public DelegateCommand HideUpdateStatusCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }

        public UpdateStatusViewModel(IUpdateService updateService)
        {
            this.updateService = updateService;

            this.DownloadOrInstallUpdateCommand = new DelegateCommand(this.DownloadOrInstallUpdate);
            this.HideUpdateStatusCommand = new DelegateCommand(() => this.updateService.Dismiss());

            this.updateService.NewVersionAvailable += NewVersionAvailableHandler;
            this.updateService.NoNewVersionAvailable += NoNewVersionAvailableHandler;

            this.LoadedCommand = new DelegateCommand(() => updateService.Reset());
        }

        private void NewVersionAvailableHandler(object sender, UpdateAvailableEventArgs e)
        {
            this.Package = e.UpdatePackage;
            this.IsUpdateAvailable = true;

            this.destinationPath = e.UpdatePackageLocation;
            RaisePropertyChanged(nameof(this.ShowInstallUpdateButton));

            if (!string.IsNullOrEmpty(destinationPath))
            {
                this.UpdateToolTip = ResourceUtils.GetString("Language_Click_Here_To_Install");
            }
            else
            {
                this.UpdateToolTip = ResourceUtils.GetString("Language_Click_Here_To_Download");
            }
        }

        private void NoNewVersionAvailableHandler(object sender, EventArgs e)
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
                Actions.TryOpenLink(UpdateInformation.DownloadLink);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not open the download link. Exception: {0}", ex.Message);
            }
        }
    }
}
