using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.UWP.ViewModels
{
    public class ViewModelLocator
    {
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
        }
        #endregion
    }
}