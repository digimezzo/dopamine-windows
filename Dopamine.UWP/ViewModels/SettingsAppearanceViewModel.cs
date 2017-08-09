using Dopamine.Core.Services.Appearance;
using Dopamine.Core.Services.Settings;
using Dopamine.UWP.Services.Appearance;
using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace Dopamine.UWP.ViewModels
{
    public class SettingsAppearanceViewModel : BindableBase
    {
        #region Variables
        private ISettingsService settingsService;
        private Services.Appearance.IAppearanceService appearanceService;
        private bool useLightTheme;
        private bool followWindowsColor;
        private ObservableCollection<ColorScheme> colorSchemes = new ObservableCollection<ColorScheme>();
        private ColorScheme selectedColorScheme;
        #endregion

        #region Properties
        public bool UseLightTheme
        {
            get { return this.useLightTheme; }
            set
            {
                SetProperty<bool>(ref this.useLightTheme, value);

                this.settingsService.UseLightTheme = value;
                this.appearanceService.ApplyTheme(value);
            }
        }

        public bool FollowWindowsColor
        {
            get { return this.followWindowsColor; }
            set
            {
                SetProperty<bool>(ref this.followWindowsColor, value);

                this.settingsService.FollowWindowsColor = value;
                this.appearanceService.ApplyColorScheme(value, this.settingsService.ColorScheme);   
            }
        }

        public ObservableCollection<ColorScheme> ColorSchemes
        {
            get { return this.colorSchemes; }
            set
            {
                SetProperty<ObservableCollection<ColorScheme>>(ref this.colorSchemes, value);
            }
        }

        public ColorScheme SelectedColorScheme
        {
            get { return this.selectedColorScheme; }

            set
            {
                SetProperty(ref this.selectedColorScheme, value);

                if (value != null)
                {
                    this.settingsService.ColorScheme = value.Name;
                    this.appearanceService.ApplyColorScheme(this.settingsService.FollowWindowsColor, value.Name);
                }
            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceViewModel(ISettingsService settingsService, Services.Appearance.IAppearanceService appearanceService)
        {
            this.settingsService = settingsService;
            this.appearanceService = appearanceService;

            // Toggle switches
            this.GetToggleSwitches();

            // ColorSchemes
            this.LoadColorSchemes();
        }
        #endregion

        #region Private
        private void LoadColorSchemes()
        {
            this.ColorSchemes.Clear();

            foreach (ColorScheme cs in this.appearanceService.GetColorSchemes())
            {
                this.ColorSchemes.Add(cs);
            }

            string savedAppearanceColorScheme = this.settingsService.ColorScheme;
            this.SelectedColorScheme = this.appearanceService.GetColorScheme(savedAppearanceColorScheme);
        }

        private void GetToggleSwitches()
        {
            this.UseLightTheme = this.settingsService.UseLightTheme;
            this.FollowWindowsColor = this.settingsService.FollowWindowsColor;
        }
        #endregion
    }
}
