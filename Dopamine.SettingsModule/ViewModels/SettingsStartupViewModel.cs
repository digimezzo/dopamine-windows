using Dopamine.Common.Services.Update;
using Dopamine.Core.Settings;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsStartupViewModel : BindableBase
    {
        #region Variables
        private bool checkBoxCheckForUpdatesChecked;
        private bool checkBoxAlsoCheckForPreReleasesChecked;
        private bool checkBoxInstallUpdatesAutomaticallyChecked;
        private bool checkBoxStartupPageChecked;
        private bool checkBoxRembemberLastPlayedTrackChecked;
        private IUpdateService updateService;
        private bool isportable;
        #endregion

        #region Properties
        public bool CheckBoxCheckForUpdatesChecked
        {
            get { return this.checkBoxCheckForUpdatesChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Updates", "CheckForUpdates", value);
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
                XmlSettingsClient.Instance.Set<bool>("Updates", "AlsoCheckForPreReleases", value);
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
                XmlSettingsClient.Instance.Set<bool>("Updates", "AutomaticDownload", value);
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
                XmlSettingsClient.Instance.Set<bool>("Startup", "ShowLastSelectedPage", value);
                SetProperty<bool>(ref this.checkBoxStartupPageChecked, value);
            }
        }

        public bool CheckBoxRembemberLastPlayedTrackChecked
        {
            get { return this.checkBoxRembemberLastPlayedTrackChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Startup", "RememberLastPlayedTrack", value);
                SetProperty<bool>(ref this.checkBoxRembemberLastPlayedTrackChecked, value);
            }
        }

        public bool IsPortable
        {
            get { return this.isportable; }
            set { SetProperty<bool>(ref this.isportable, value); }
        }
        #endregion

        #region Construction
        public SettingsStartupViewModel(IUpdateService updateService)
        {
            this.updateService = updateService;

            this.IsPortable = XmlSettingsClient.Instance.BaseGet<bool>("Application", "IsPortable");

            // CheckBoxes
            this.GetCheckBoxesAsync();

            // No automatic updates in the portable version
            if (!this.IsPortable)
            {
                this.CheckBoxInstallUpdatesAutomaticallyChecked = XmlSettingsClient.Instance.Get<bool>("Updates", "AutomaticDownload");
            }
            else
            {
                this.CheckBoxInstallUpdatesAutomaticallyChecked = false;
            }
        }
        #endregion

        #region Private
        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxCheckForUpdatesChecked = XmlSettingsClient.Instance.Get<bool>("Updates", "CheckForUpdates");
                this.CheckBoxAlsoCheckForPreReleasesChecked = XmlSettingsClient.Instance.Get<bool>("Updates", "AlsoCheckForPreReleases");
                this.CheckBoxStartupPageChecked = XmlSettingsClient.Instance.Get<bool>("Startup", "ShowLastSelectedPage");
                this.CheckBoxRembemberLastPlayedTrackChecked = XmlSettingsClient.Instance.Get<bool>("Startup", "RememberLastPlayedTrack");
            });
        }
        #endregion
    }
}
