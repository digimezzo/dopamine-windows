using Dopamine.Core.Services.Appearance;
using Dopamine.Core.Services.Settings;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Views
{
    public sealed partial class Main : Page
    {
        #region Variables
        private IAppearanceService appearanceService;
        private ISettingsService settingsService;
        #endregion

        #region Construction
        public Main(ISettingsService settingsService, IAppearanceService appearanceService)
        {
            this.InitializeComponent();

            this.appearanceService = appearanceService;
            this.appearanceService.ThemeChanged += AppearanceService_ThemeChanged;

            this.settingsService = settingsService;
            this.appearanceService.ApplyTheme(this.settingsService.UseLightTheme);
            this.appearanceService.ApplyColorSchemeAsync(this.settingsService.ColorScheme, this.settingsService.FollowWindowsColor, false);
        }
        #endregion

        #region Private
        private void AppearanceService_ThemeChanged(bool useLightTheme)
        {
            this.RequestedTheme = useLightTheme ? Windows.UI.Xaml.ElementTheme.Light : Windows.UI.Xaml.ElementTheme.Dark;
        }
        #endregion
    }
}