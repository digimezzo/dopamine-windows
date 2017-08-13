using System;
using System.Collections.Generic;
using Dopamine.Core.Services.Appearance;
using Dopamine.Core.Services.Logging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Dopamine.UWP.Utils;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using System.Threading.Tasks;

namespace Dopamine.UWP.Services.Appearance
{
    public class AppearanceService : IAppearanceService
    {
        #region Variables
        private ILoggingService loggingService;
        private bool followWindowsColor = false;
        private ColorScheme[] colorSchemes = {
                                                new ColorScheme {
                                                    Name = "Blue",
                                                    AccentColor = "#1D7DD4"
                                                },
                                                new ColorScheme {
                                                    Name = "Green",
                                                    AccentColor = "#7FB718"
                                                },
                                                new ColorScheme {
                                                    Name = "Yellow",
                                                    AccentColor = "#F09609"
                                                },
                                                new ColorScheme {
                                                    Name = "Purple",
                                                    AccentColor = "#A835B2"
                                                },
                                                new ColorScheme {
                                                    Name = "Pink",
                                                    AccentColor = "#CE0058"
                                                },
                                                new ColorScheme {
                                                    Name = "Red",
                                                    AccentColor = "#E31837"
                                                }
        };
        #endregion

        #region Construction
        public AppearanceService(ILoggingService loggingService)
        {
            this.loggingService = loggingService;
        }
        #endregion

        #region IAppearanceService
        public event ThemeChangedEventHandler ThemeChanged = delegate { };
        public event EventHandler ColorSchemeChanged = delegate { };
        public event EventHandler ColorSchemesChanged = delegate { };

        public async Task ApplyColorScheme(bool followWindowsColor, bool followAlbumCoverColor = false, bool isViewModelLoaded = false, string selectedColorScheme = "")
        {
            this.followWindowsColor = followWindowsColor;
            Color accentColor = default(Color);

            await Task.Run(() =>
            {
               

                if (this.followWindowsColor)
                {
                    accentColor = (Color)Application.Current.Resources["SystemAccentColor"];
                }
                else
                {
                    ColorScheme cs = this.GetColorScheme(selectedColorScheme);
                    accentColor = ColorUtils.ConvertStringToColor(cs.AccentColor);
                }
            });

            Application.Current.Resources["Color_Accent"] = accentColor;
            ((SolidColorBrush)Application.Current.Resources["Brush_Accent"]).Color = accentColor;
            ((SolidColorBrush)Application.Current.Resources["Brush_AccentMedium"]).Color = accentColor;
        }

        public void ApplyTheme(bool enableLightTheme)
        {
            this.SetTitleBarTheme(enableLightTheme);
            this.SetStatusBarTheme(enableLightTheme);
            this.ThemeChanged(enableLightTheme);
        }

        public ColorScheme GetColorScheme(string name)
        {
            // Set the default theme in case the theme is not found by using the For loop
            ColorScheme returnVal = this.colorSchemes[0];

            foreach (ColorScheme item in this.colorSchemes)
            {
                if (item.Name == name)
                {
                    returnVal = item;
                }
            }

            return returnVal;
        }

        public List<ColorScheme> GetColorSchemes()
        {
            return this.colorSchemes.ToList();
        }
        #endregion

        #region Private
        private void SetTitleBarTheme(bool enableLightTheme)
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            Color foregroundColor = Colors.White;
            Color backgroundColor = Colors.Black;

            if (enableLightTheme)
            {
                foregroundColor = Colors.Black;
                backgroundColor = Colors.White;
            }

            titleBar.BackgroundColor = backgroundColor;
            titleBar.ForegroundColor = foregroundColor;
            titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.InactiveForegroundColor = foregroundColor;
            titleBar.ButtonBackgroundColor = backgroundColor;
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.ButtonInactiveBackgroundColor = backgroundColor;
            titleBar.ButtonInactiveForegroundColor = foregroundColor;
        }

        private void SetStatusBarTheme(bool enableLightTheme)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                Color foregroundColor = Colors.White;
                Color backgroundColor = Colors.Black;

                if (enableLightTheme)
                {
                    foregroundColor = Colors.Black;
                    backgroundColor = Colors.White;
                }

                statusBar.BackgroundColor = backgroundColor;
                statusBar.ForegroundColor = foregroundColor;
                statusBar.BackgroundOpacity = 1;
            }
        }

        public void WatchWindowsColor(object window)
        {
            // No implementation required here
        }
        #endregion
    }
}