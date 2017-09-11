using Dopamine.Core.Services.Appearance;
using Dopamine.UWP.Settings;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Views
{
    public sealed partial class Main : Page
    {
        #region Variables
        private IAppearanceService appearanceService;
        private IMergedSettings settings;
        #endregion

        #region Construction
        public Main(IAppearanceService appearanceService, IMergedSettings settings)
        {
            this.InitializeComponent();

            this.settings = settings;

            this.appearanceService = appearanceService;
            this.appearanceService.ThemeChanged += AppearanceService_ThemeChanged;

            this.appearanceService.ApplyTheme(this.settings.UseLightTheme);
            this.appearanceService.ApplyColorSchemeAsync(this.settings.ColorScheme, this.settings.FollowWindowsColor, false);
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