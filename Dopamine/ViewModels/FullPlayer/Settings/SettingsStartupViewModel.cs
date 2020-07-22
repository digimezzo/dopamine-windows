using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Dopamine.Services.Update;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsStartupViewModel : BindableBase
    {
        private bool checkBoxCheckForUpdatesAtStartupChecked;
        private bool checkBoxCheckForUpdatesPeriodicallyChecked;
        private bool checkBoxInstallUpdatesAutomaticallyChecked;
        private bool checkBoxStartupPageChecked;
        private bool checkBoxRememberLastPlayedTrackChecked;
        private bool checkBoxCheckForPrereleasesChecked;
        private IUpdateService updateService;
        private bool isportable;

        public bool IsUpdateCheckEnabled
        {
            get { return this.checkBoxCheckForUpdatesAtStartupChecked | checkBoxCheckForUpdatesPeriodicallyChecked; }
        }

        public bool CheckBoxCheckForUpdatesAtStartupChecked
        {
            get { return this.checkBoxCheckForUpdatesAtStartupChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckAtStartup", value);
                SetProperty<bool>(ref this.checkBoxCheckForUpdatesAtStartupChecked, value);
                RaisePropertyChanged(nameof(this.IsUpdateCheckEnabled));
                this.updateService.Reset();
            }
        }

        public bool CheckBoxCheckForUpdatesPeriodicallyChecked
        {
            get { return this.checkBoxCheckForUpdatesPeriodicallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckPeriodically", value);
                SetProperty<bool>(ref this.checkBoxCheckForUpdatesPeriodicallyChecked, value);
                RaisePropertyChanged(nameof(this.IsUpdateCheckEnabled));
                this.updateService.Reset();
            }
        }

        public bool CheckBoxInstallUpdatesAutomaticallyChecked
        {
            get { return this.checkBoxInstallUpdatesAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "AutomaticDownload", value);
                SetProperty<bool>(ref this.checkBoxInstallUpdatesAutomaticallyChecked, value);
                this.updateService.Reset();
            }
        }

        public bool CheckBoxCheckForPrereleasesChecked
        {
            get { return this.checkBoxCheckForPrereleasesChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckForPrereleases", value);
                SetProperty<bool>(ref this.checkBoxCheckForPrereleasesChecked, value);
                this.updateService.Reset();
            }
        }

        public bool CheckBoxStartupPageChecked
        {
            get { return this.checkBoxStartupPageChecked; }
            set
            {
                SettingsClient.Set<bool>("Startup", "ShowLastSelectedPage", value);
                SetProperty<bool>(ref this.checkBoxStartupPageChecked, value);
            }
        }

        public bool CheckBoxRememberLastPlayedTrackChecked
        {
            get { return this.checkBoxRememberLastPlayedTrackChecked; }
            set
            {
                SettingsClient.Set<bool>("Startup", "RememberLastPlayedTrack", value);
                SetProperty<bool>(ref this.checkBoxRememberLastPlayedTrackChecked, value);
            }
        }

        public bool IsPortable
        {
            get { return this.isportable; }
            set { SetProperty<bool>(ref this.isportable, value); }
        }

        public SettingsStartupViewModel(IUpdateService updateService)
        {
            this.updateService = updateService;

            this.IsPortable = SettingsClient.Get<bool>("Configuration", "IsPortable");

            // CheckBoxes
            this.GetCheckBoxesAsync();

            // No automatic updates in the portable version
            if (!this.IsPortable)
            {
                this.checkBoxInstallUpdatesAutomaticallyChecked = SettingsClient.Get<bool>("Updates", "AutomaticDownload");
            }
            else
            {
                this.checkBoxInstallUpdatesAutomaticallyChecked = false;
            }
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxCheckForUpdatesAtStartupChecked = SettingsClient.Get<bool>("Updates", "CheckAtStartup");
                this.checkBoxCheckForPrereleasesChecked = SettingsClient.Get<bool>("Updates", "CheckForPrereleases");
                this.checkBoxCheckForUpdatesPeriodicallyChecked = SettingsClient.Get<bool>("Updates", "CheckPeriodically");
                this.checkBoxStartupPageChecked = SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage");
                this.checkBoxRememberLastPlayedTrackChecked = SettingsClient.Get<bool>("Startup", "RememberLastPlayedTrack");
            });
        }
    }
}
