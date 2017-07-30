using Dopamine.UWP.Services.Appearance;
using GalaSoft.MvvmLight.Ioc;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Views
{
    public class ThemeablePage : Page
    {
        #region Variables
        private IAppearanceService appearanceService;
        #endregion

        #region Construction
        public ThemeablePage()
        {
            this.appearanceService = SimpleIoc.Default.GetInstance<IAppearanceService>();
            this.appearanceService.ThemeChanged += AppearanceService_ThemeChanged;
        }

        private void AppearanceService_ThemeChanged(bool useLightTheme)
        {
            this.RequestedTheme = useLightTheme ? Windows.UI.Xaml.ElementTheme.Light : Windows.UI.Xaml.ElementTheme.Dark;
        }
        #endregion
    }
}
