using Dopamine.Common.Services.Appearance;
using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Mvvm;
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
                XmlSettingsClient.Instance.Set<bool>("Appearance", "EnableLightTheme", value);
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
                    XmlSettingsClient.Instance.Set<string>("Appearance", "ColorScheme", value.Name);
                    Application.Current.Dispatcher.Invoke(() => this.appearanceService.ApplyColorScheme(XmlSettingsClient.Instance.Get<bool>("Appearance", "FollowWindowsColor"), value.Name));
                }

                SetProperty<ColorScheme>(ref this.selectedColorScheme, value);
            }
        }

        public bool CheckBoxWindowsColorChecked
        {
            get { return this.checkBoxWindowsColorChecked; }

            set
            {
                XmlSettingsClient.Instance.Set<bool>("Appearance", "FollowWindowsColor", value);
                Application.Current.Dispatcher.Invoke(() => this.appearanceService.ApplyColorScheme(value, XmlSettingsClient.Instance.Get<string>("Appearance", "ColorScheme")));

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

            string savedColorSchemeName = XmlSettingsClient.Instance.Get<string>("Appearance", "ColorScheme");

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
                this.CheckBoxWindowsColorChecked = XmlSettingsClient.Instance.Get<bool>("Appearance", "FollowWindowsColor");
                this.CheckBoxThemeChecked = XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme");
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
