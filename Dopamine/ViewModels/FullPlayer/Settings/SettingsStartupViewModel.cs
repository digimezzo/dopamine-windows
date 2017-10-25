using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Update;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsStartupViewModel : BindableBase
    {
        private bool checkBoxCheckForUpdatesChecked;
        private bool checkBoxAlsoCheckForPreReleasesChecked;
        private bool checkBoxInstallUpdatesAutomaticallyChecked;
        private bool checkBoxStartupPageChecked;
        private bool checkBoxRembemberLastPlayedTrackChecked;
        private IUpdateService updateService;
        private bool isportable;

        public bool CheckBoxCheckForUpdatesChecked
        {
            get { return this.checkBoxCheckForUpdatesChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "CheckForUpdates", value);
                SetProperty<bool>(ref this.checkBoxCheckForUpdatesChecked, value);

                if (value)
                {
                    this.updateService.EnableUpdateCheck();
                }
                else
                {
                    this.updateService.DisableUpdateCheck();
                }
            }
        }

        public bool CheckBoxAlsoCheckForPreReleasesChecked
        {
            get { return this.checkBoxAlsoCheckForPreReleasesChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "AlsoCheckForPreReleases", value);
                SetProperty<bool>(ref this.checkBoxAlsoCheckForPreReleasesChecked, value);

                if (this.CheckBoxCheckForUpdatesChecked)
                {
                    this.updateService.EnableUpdateCheck();
                }
                else
                {
                    this.updateService.DisableUpdateCheck();
                }
            }
        }

        public bool CheckBoxInstallUpdatesAutomaticallyChecked
        {
            get { return this.checkBoxInstallUpdatesAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Updates", "AutomaticDownload", value);
                SetProperty<bool>(ref this.checkBoxInstallUpdatesAutomaticallyChecked, value);

                if (this.CheckBoxCheckForUpdatesChecked)
                {
                    this.updateService.EnableUpdateCheck();
                }
                else
                {
                    this.updateService.DisableUpdateCheck();
                }
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
                this.checkBoxCheckForUpdatesChecked = SettingsClient.Get<bool>("Updates", "CheckForUpdates");
                this.checkBoxAlsoCheckForPreReleasesChecked = SettingsClient.Get<bool>("Updates", "AlsoCheckForPreReleases");
                this.checkBoxStartupPageChecked = SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage");
                this.checkBoxRembemberLastPlayedTrackChecked = SettingsClient.Get<bool>("Startup", "RememberLastPlayedTrack");
            });
        }
    }
}
