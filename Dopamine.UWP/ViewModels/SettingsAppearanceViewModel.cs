using Dopamine.Core.Services.Settings;
using Dopamine.UWP.Services.Appearance;
using GalaSoft.MvvmLight;

namespace Dopamine.UWP.ViewModels
{
    public class SettingsAppearanceViewModel : ViewModelBase
    {
        #region Variables
        private ISettingsService settingsService;
        private IAppearanceService appearanceService;
        private bool useLightTheme;
        private bool followWindowsColor;
        #endregion

        #region Properties
        public bool UseLightTheme
        {
            get { return this.useLightTheme; }
            set
            {
                this.useLightTheme = value;
                this.settingsService.UseLightTheme = value;
                this.appearanceService.ApplyTheme(value);
                this.RaisePropertyChanged(() => this.UseLightTheme);
            }
        }

        public bool FollowWindowsColor
        {
            get { return this.useLightTheme; }
            set
            {
                this.followWindowsColor = value;
                this.settingsService.FollowWindowsColor = value;
                this.appearanceService.ApplyColorScheme(value, this.settingsService.ColorScheme);
                this.RaisePropertyChanged(() => this.FollowWindowsColor);
            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceViewModel(ISettingsService settingsService, IAppearanceService appearanceService)
        {
            this.settingsService = settingsService;
            this.appearanceService = appearanceService;

            // Toggle switches
            this.GetToggleSwitches();
        }
        #endregion

        #region Private
        private void GetToggleSwitches()
        {
            this.UseLightTheme = this.settingsService.UseLightTheme;
            this.FollowWindowsColor = this.settingsService.FollowWindowsColor;
        }
        #endregion
    }
}
