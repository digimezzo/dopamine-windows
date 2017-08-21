using Dopamine.Core.Services.Appearance;
using Dopamine.UWP.Utils;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Dopamine.UWP.Services.Appearance
{
    public class AppearanceService :  Core.Services.Appearance.AppearanceService
    {
        #region Construction
        public AppearanceService() : base()
        {
            // Get the available ColorSchemes
            // ------------------------------
            this.GetAllColorSchemes();
        }
        #endregion

        #region Overrides
        public override async Task ApplyColorSchemeAsync(string selectedColorScheme, bool followWindowsColor, bool followAlbumCoverColor, bool isViewModelLoaded = false)
        {
            this.FollowWindowsColor = followWindowsColor;
            Color accentColor = default(Color);

            if (this.FollowWindowsColor)
            {
                accentColor = (Color)Application.Current.Resources["SystemAccentColor"];
            }
            else
            {
                await Task.Run(() =>
                {
                    ColorScheme cs = this.GetColorScheme(selectedColorScheme);
                    accentColor = ColorUtils.ConvertStringToColor(cs.AccentColor);
                });
            }

            Application.Current.Resources["Color_Accent"] = accentColor;
            ((SolidColorBrush)Application.Current.Resources["Brush_Accent"]).Color = accentColor;
            ((SolidColorBrush)Application.Current.Resources["Brush_AccentMedium"]).Color = accentColor;
        }

        public override void ApplyTheme(bool enableLightTheme)
        {
            this.SetTitleBarTheme(enableLightTheme);
            this.SetStatusBarTheme(enableLightTheme);
            this.OnThemeChanged(enableLightTheme);
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

        public override void WatchWindowsColor(object window)
        {
            throw new NotImplementedException();
        }

        protected override void GetAllColorSchemes()
        {
            this.GetBuiltInColorSchemes();
        }
        #endregion
    }
}