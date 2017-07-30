using Dopamine.Core.Services.Settings;
using Dopamine.UWP.Services.Appearance;
using GalaSoft.MvvmLight.Ioc;
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
        public Main()
        {
            this.InitializeComponent();

            this.appearanceService = SimpleIoc.Default.GetInstance<IAppearanceService>();
            this.appearanceService.ThemeChanged += AppearanceService_ThemeChanged;

            this.settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            this.appearanceService.ApplyTheme(this.settingsService.UseLightTheme);
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