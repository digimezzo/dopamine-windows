using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.UWP.ViewModels
{
    public class ViewModelLocator
    {
        #region Properties
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public SettingsAppearanceViewModel SettingsAppearance => ServiceLocator.Current.GetInstance<SettingsAppearanceViewModel>();
        #endregion

        #region Construction
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            this.RegisterViewModels();
        }
        #endregion

        #region Private
        private void RegisterViewModels()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsAppearanceViewModel>();
        }
        #endregion
    }
}