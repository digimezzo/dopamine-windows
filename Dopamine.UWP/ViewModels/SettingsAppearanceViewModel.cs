using Dopamine.Core.Services.Appearance;
using Dopamine.UWP.Settings;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Dopamine.UWP.ViewModels
{
    public class SettingsAppearanceViewModel : BindableBase
    {
        #region Variables
        private IAppearanceService appearanceService;
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

                SettingsClient.UseLightTheme = value;
                this.appearanceService.ApplyTheme(value);
            }
        }

        public bool FollowWindowsColor
        {
            get { return this.followWindowsColor; }
            set
            {
                SetProperty<bool>(ref this.followWindowsColor, value);

                SettingsClient.FollowWindowsColor = value;
                this.appearanceService.ApplyColorSchemeAsync(SettingsClient.ColorScheme, value, false);   
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
                    SettingsClient.ColorScheme = value.Name;
                    this.appearanceService.ApplyColorSchemeAsync(value.Name, SettingsClient.FollowWindowsColor, false);
                }
            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceViewModel(IAppearanceService appearanceService)
        {
            this.appearanceService = appearanceService;

            this.LoadColorSchemes();
            this.GetToggleSwitches();
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

            string savedAppearanceColorScheme = SettingsClient.ColorScheme;
            this.SelectedColorScheme = this.appearanceService.GetColorScheme(savedAppearanceColorScheme);
        }

        private void GetToggleSwitches()
        {
            this.useLightTheme = SettingsClient.UseLightTheme;
            this.RaisePropertyChanged(nameof(this.UseLightTheme));

            this.followWindowsColor = SettingsClient.FollowWindowsColor;
            this.RaisePropertyChanged(nameof(this.FollowWindowsColor));
        }
        #endregion
    }
}
