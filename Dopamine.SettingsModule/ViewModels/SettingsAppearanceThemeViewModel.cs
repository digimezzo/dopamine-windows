using Dopamine.Common.Settings;
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
        private ISettings settings;
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
                this.settings.UseLightTheme = value;
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
                    this.settings.ColorScheme = value.Name;
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await this.appearanceService.ApplyColorSchemeAsync(
                            value.Name,
                            this.settings.FollowWindowsColor,
                            this.settings.FollowAlbumCoverColor,
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

                this.settings.FollowWindowsColor = value;

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await this.appearanceService.ApplyColorSchemeAsync(
                        this.settings.ColorScheme,
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

                this.settings.FollowAlbumCoverColor = value;

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await this.appearanceService.ApplyColorSchemeAsync(
                          this.settings.ColorScheme,
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
        public SettingsAppearanceThemeViewModel(ISettings settings, IAppearanceService appearanceService)
        {
            this.settings = settings;
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

            string savedColorSchemeName = this.settings.ColorScheme;

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
                this.checkBoxThemeChecked = this.settings.UseLightTheme;
                this.RaisePropertyChanged(nameof(this.CheckBoxThemeChecked));

                this.checkBoxWindowsColorChecked = this.settings.FollowWindowsColor;
                this.RaisePropertyChanged(nameof(this.CheckBoxWindowsColorChecked));

                this.checkBoxAlbumCoverColorChecked = this.settings.FollowAlbumCoverColor;
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
