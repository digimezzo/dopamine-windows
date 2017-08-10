using Dopamine.Core.Services.Settings;
using Dopamine.UWP.Services.Appearance;
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
            this.appearanceService.ApplyColorScheme(this.settingsService.FollowWindowsColor, this.settingsService.ColorScheme);
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