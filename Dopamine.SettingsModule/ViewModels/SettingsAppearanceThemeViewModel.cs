using Digimezzo.Utilities.Settings;
using Dopamine.Common.Settings;
using Dopamine.Core.Services.Appearance;
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
        private ObservableCollection<ColorScheme> colorSchemes = new ObservableCollection<ColorScheme>();
        private ColorScheme selectedColorScheme;
        private bool checkBoxWindowsColorChecked;
        private bool checkBoxAlbumCoverColorChecked;
        private bool checkBoxThemeChecked;
        private bool isViewModelLoaded = true;
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
                CoreSettings.Current.UseLightTheme = value;
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
                    CoreSettings.Current.ColorScheme = value.Name;
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await this.appearanceService.ApplyColorSchemeAsync(
                            value.Name,
                            CoreSettings.Current.FollowWindowsColor,
                            SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor"),
                            isViewModelLoaded
                            );
                        isViewModelLoaded = false;
                    });
                }

                SetProperty<ColorScheme>(ref this.selectedColorScheme, value);
            }
        }

        public bool CheckBoxWindowsColorChecked
        {
            get { return this.checkBoxWindowsColorChecked; }

            set
            {
                if (value)
                {
                    this.CheckBoxAlbumCoverColorChecked = false;
                }

                CoreSettings.Current.FollowWindowsColor = value;

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await this.appearanceService.ApplyColorSchemeAsync(
                        CoreSettings.Current.ColorScheme,
                        value,
                        false,
                        isViewModelLoaded
                        );
                });

                SetProperty<bool>(ref this.checkBoxWindowsColorChecked, value);
                this.RaisePropertyChanged(nameof(this.CanChooseColor));
            }
        }

        public bool CheckBoxAlbumCoverColorChecked
        {
            get { return this.checkBoxAlbumCoverColorChecked; }

            set
            {
                if (value)
                {
                    this.CheckBoxWindowsColorChecked = false;
                }

                SettingsClient.Set<bool>("Appearance", "FollowAlbumCoverColor", value);

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await this.appearanceService.ApplyColorSchemeAsync(
                          CoreSettings.Current.ColorScheme,
                          false,
                          value,
                          isViewModelLoaded
                          );
                });

                SetProperty<bool>(ref this.checkBoxAlbumCoverColorChecked, value);
                this.RaisePropertyChanged(nameof(this.CanChooseColor));
            }
        }

        public bool CanChooseColor
        {
            get { return !this.CheckBoxWindowsColorChecked & !this.CheckBoxAlbumCoverColorChecked; }
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

            await Task.Run(() =>
            {
                foreach (ColorScheme cs in this.appearanceService.GetColorSchemes())
                {
                    localColorSchemes.Add(cs);
                }
            });

            this.ColorSchemes = localColorSchemes;

            string savedColorSchemeName = CoreSettings.Current.ColorScheme;

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
            await Task.Run(() =>
            {
                this.checkBoxThemeChecked = CoreSettings.Current.UseLightTheme;
                this.RaisePropertyChanged(nameof(this.CheckBoxThemeChecked));

                this.checkBoxWindowsColorChecked = CoreSettings.Current.FollowWindowsColor;
                this.RaisePropertyChanged(nameof(this.CheckBoxWindowsColorChecked));

                this.checkBoxAlbumCoverColorChecked = SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor");
                this.RaisePropertyChanged(nameof(this.CheckBoxAlbumCoverColorChecked));
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
