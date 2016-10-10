using Dopamine.Common.Services.Dialog;
using Dopamine.Core.Utils;
using Dopamine.InformationModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Microsoft.Practices.Unity;

namespace Dopamine.InformationModule.ViewModels
{
    public class InformationAboutViewModel : BindableBase
    {
        #region Variables
        private IUnityContainer container;
        private IDialogService dialogService;
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

                this.dialogService.ShowCustomDialog(
                    0xe73e, 
                    16, 
                    ResourceUtils.GetStringResource("Language_License"), 
                    view, 
                    400, 
                    0, 
                    false,
                    true,
                    true, 
                    false, 
                    ResourceUtils.GetStringResource("Language_Ok"), 
                    string.Empty,
                    null);
            });
        }
        #endregion
    }
}
