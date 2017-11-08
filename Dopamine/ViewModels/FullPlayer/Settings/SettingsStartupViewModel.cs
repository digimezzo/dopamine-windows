using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Update;
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
        private bool checkBoxRembemberLastPlayedTrackChecked;
        private IUpdateService updateService;
        private bool isportable;

        public bool CheckBoxCheckForUpdatesAtStartupChecked
        {
            get { return this.checkBoxCheckForUpdatesAtStartupChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckAtStartup", value);
                SetProperty<bool>(ref this.checkBoxCheckForUpdatesAtStartupChecked, value);
            }
        }

        public bool CheckBoxCheckForUpdatesPeriodicallyChecked
        {
            get { return this.checkBoxCheckForUpdatesPeriodicallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckPeriodically", value);
                SetProperty<bool>(ref this.checkBoxCheckForUpdatesPeriodicallyChecked, value);
            }
        }

        public bool CheckBoxInstallUpdatesAutomaticallyChecked
        {
            get { return this.checkBoxInstallUpdatesAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "AutomaticDownload", value);
                SetProperty<bool>(ref this.checkBoxInstallUpdatesAutomaticallyChecked, value);
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

        public bool CheckBoxRembemberLastPlayedTrackChecked
        {
            get { return this.checkBoxRembemberLastPlayedTrackChecked; }
            set
            {
                SettingsClient.Set<bool>("Startup", "RememberLastPlayedTrack", value);
                SetProperty<bool>(ref this.checkBoxRembemberLastPlayedTrackChecked, value);
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
                this.CheckBoxInstallUpdatesAutomaticallyChecked = SettingsClient.Get<bool>("Updates", "AutomaticDownload");
            }
            else
            {
                this.CheckBoxInstallUpdatesAutomaticallyChecked = false;
            }
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxCheckForUpdatesAtStartupChecked = SettingsClient.Get<bool>("Updates", "CheckAtStartup");
                this.checkBoxCheckForUpdatesPeriodicallyChecked = SettingsClient.Get<bool>("Updates", "CheckPeriodically");
                this.checkBoxStartupPageChecked = SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage");
                this.checkBoxRembemberLastPlayedTrackChecked = SettingsClient.Get<bool>("Startup", "RememberLastPlayedTrack");
            });
        }
    }
}
