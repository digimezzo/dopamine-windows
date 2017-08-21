using Dopamine.Core.Services.Appearance;
using Dopamine.UWP.Settings;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Views
{
    public sealed partial class Main : Page
    {
        #region Variables
        private IAppearanceService appearanceService;
        #endregion

        #region Construction
        public Main(IAppearanceService appearanceService)
        {
            this.InitializeComponent();

            this.appearanceService = appearanceService;
            this.appearanceService.ThemeChanged += AppearanceService_ThemeChanged;

            this.appearanceService.ApplyTheme(CoreSettings.Current.UseLightTheme);
            this.appearanceService.ApplyColorSchemeAsync(CoreSettings.Current.ColorScheme, CoreSettings.Current.FollowWindowsColor, false);
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