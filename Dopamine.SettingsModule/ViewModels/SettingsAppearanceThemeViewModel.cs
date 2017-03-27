using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Appearance;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsAppearanceThemeViewModel : BindableBase
    {
        #region Variables
        private IAppearanceService appearanceService;
        private ObservableCollection<string> themes = new ObservableCollection<string>();
        private string selectedTheme;
        private ObservableCollection<ColorScheme> colorSchemes = new ObservableCollection<ColorScheme>();
        private ColorScheme selectedColorScheme;
        private bool checkBoxWindowsColorChecked;
        private bool checkBoxThemeChecked;
        #endregion

        #region Properties
        public ObservableCollection<string> Themes
        {
            get { return this.themes; }
            set { SetProperty<ObservableCollection<string>>(ref this.themes, value); }
        }

        public bool CheckBoxThemeChecked
        {
            get { return this.checkBoxThemeChecked; }
            set
            {
                SettingsClient.Set<bool>("Appearance", "EnableLightTheme", value);
                Application.Current.Dispatcher.Invoke(() => this.appearanceService.ApplyTheme(value));
                SetProperty<bool>(ref this.checkBoxThemeChecked, value);
            }
        }

        public ObservableCollection<ColorScheme> ColorSchemes
        {
            get { return this.colorSchemes; }
            set { SetProperty<ObservableCollection<ColorScheme>>(ref this.colorSchemes, value); }
        }

        public ColorScheme SelectedColorScheme
        {
            get { return this.selectedColorScheme; }

            set
            {
                // value can be Nothing when a ColorScheme is removed from the ColorSchemes directory
                if (value != null)
                {
                    SettingsClient.Set<string>("Appearance", "ColorScheme", value.AccentColor);
                    Application.Current.Dispatcher.Invoke(() => this.appearanceService.ApplyColorScheme(SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"), value.AccentColor));
                }

                SetProperty<ColorScheme>(ref this.selectedColorScheme, value);
            }
        }

        public bool CheckBoxWindowsColorChecked
        {
            get { return this.checkBoxWindowsColorChecked; }

            set
            {
                SettingsClient.Set<bool>("Appearance", "FollowWindowsColor", value);
                Application.Current.Dispatcher.Invoke(() => this.appearanceService.ApplyColorScheme(value, SettingsClient.Get<string>("Appearance", "ColorScheme")));

                SetProperty<bool>(ref this.checkBoxWindowsColorChecked, value);
            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceThemeViewModel(IAppearanceService appearanceService)
        {
            this.appearanceService = appearanceService;

            this.GetColorSchemesAsync();
            this.GetCheckBoxesAsync();

            this.appearanceService.ColorSchemesChanged += ColorSchemesChangedHandler;
        }
        #endregion

        #region Private
        private async void GetColorSchemesAsync()
        {
            ObservableCollection<ColorScheme> localColorSchemes = new ObservableCollection<ColorScheme>();

            await Task.Run(() => {
                foreach (ColorScheme cs in this.appearanceService.GetColorSchemes())
                {
                    localColorSchemes.Add(cs);
                }
            });

            this.ColorSchemes = localColorSchemes;

            string savedColorSchemeName = SettingsClient.Get<string>("Appearance", "ColorScheme");

            if (!string.IsNullOrEmpty(savedColorSchemeName))
            {
                this.SelectedColorScheme = this.appearanceService.GetColorScheme(savedColorSchemeName);
            }
            else
            {
                this.SelectedColorScheme = this.appearanceService.GetColorSchemes()[0];
            }
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() => {
                this.CheckBoxWindowsColorChecked = SettingsClient.Get<bool>("Appearance", "FollowWindowsColor");
                this.CheckBoxThemeChecked = SettingsClient.Get<bool>("Appearance", "EnableLightTheme");
            });
        }
        #endregion

        #region Event handlers
        private void ColorSchemesChangedHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => this.GetColorSchemesAsync());
        }
        #endregion
    }
}
