using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Packaging;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Dialog;
using Dopamine.Core.Base;
using Dopamine.InformationModule.Views;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.InformationModule.ViewModels
{
    public class InformationAboutViewModel : BindableBase
    {
        #region Variables
        private IUnityContainer container;
        private IDialogService dialogService;
        private Package package;
        #endregion

        #region Commands
        public DelegateCommand ShowLicenseCommand { get; set; }
        #endregion

        #region Properties
        public string PayPalLink => ContactInformation.PayPalLink;
        public string WebsiteLink => ContactInformation.WebsiteLink;
        public string WebsiteContactLink => ContactInformation.WebsiteContactLink;
        public string FacebookLink => ContactInformation.FacebookLink;
        public string TwitterLink => ContactInformation.TwitterLink;

        public Package Package
        {
            get { return this.package; }
            set { SetProperty<Package>(ref this.package, value); }
        }
        #endregion

        #region Construction
        public InformationAboutViewModel(IUnityContainer container, IDialogService dialogService)
        {
            this.container = container;
            this.dialogService = dialogService;

            Configuration config;
#if DEBUG
            config = Configuration.Debug;
#else
		    config = Configuration.Release;
#endif

            this.Package = new Package(ProcessExecutable.Name(), ProcessExecutable.AssemblyVersion(), config);

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
