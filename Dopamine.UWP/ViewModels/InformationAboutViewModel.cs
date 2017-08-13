using Dopamine.Core.ViewModels;
using Dopamine.UWP.Base;
using Dopamine.UWP.Services.Dialog;
using Dopamine.UWP.Views;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Windows.ApplicationModel.Resources;

namespace Dopamine.UWP.ViewModels
{
    public sealed class InformationAboutViewModel : InformationAboutViewModelBase
    {
        #region Variables
        private IUnityContainer container;
        private IDialogService dialogService;
        ResourceLoader loader = ResourceLoader.GetForCurrentView();
        #endregion

        #region Properties
        public string AssemblyVersion => ProductInformation.AssemblyVersion;
        #endregion

        #region Commands
        public DelegateCommand ShowLicenseCommand { get; set; }
        #endregion

        #region Construction
        public InformationAboutViewModel(IUnityContainer container, IDialogService dialogService)
        {
            this.container = container;
            this.dialogService = dialogService;

            this.ShowLicenseCommand = new DelegateCommand(() =>
            {
                var view = this.container.Resolve<InformationAboutLicense>();

                this.dialogService.ShowContentDialogAsync(
                    loader.GetString("LicenseDialogLicense"), 
                    view, 
                    loader.GetString("LicenseDialogOk"), 
                    string.Empty);
            });
        }
        #endregion
    }
}