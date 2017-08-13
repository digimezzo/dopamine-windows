using Dopamine.Core.ViewModels;
using Dopamine.UWP.Base;
using Dopamine.UWP.Views;
using Microsoft.Practices.Unity;
using Prism.Commands;

namespace Dopamine.UWP.ViewModels
{
    public sealed class InformationAboutViewModel : InformationAboutViewModelBase
    {
        #region Variables
        private IUnityContainer container;
        #endregion

        #region Properties
        public string AssemblyVersion => ProductInformation.AssemblyVersion;
        #endregion

        #region Commands
        public DelegateCommand ShowLicenseCommand { get; set; }
        #endregion

        #region Construction
        public InformationAboutViewModel(IUnityContainer container)
        {
            this.container = container;

            this.ShowLicenseCommand = new DelegateCommand(() =>
            {
                var view = this.container.Resolve<InformationAboutLicense>();

                //this.dialogService.ShowCustomDialog(
                //    0xe73e,
                //    16,
                //    ResourceUtils.GetStringResource("Language_License"),
                //    view,
                //    400,
                //    0,
                //    false,
                //    true,
                //    true,
                //    false,
                //    ResourceUtils.GetStringResource("Language_Ok"),
                //    string.Empty,
                //    null);
            });
        }
        #endregion
    }
}