using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Packaging;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Services.Dialog;
using Dopamine.Views.FullPlayer.Information;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationAboutViewModel : BindableBase
    {
        private IUnityContainer container;
        private IDialogService dialogService;
        private Package package;

        public DelegateCommand ShowLicenseCommand { get; set; }
        public DelegateCommand<string> OpenLinkCommand { get; set; }

        public Package Package
        {
            get { return this.package; }
            set { SetProperty<Package>(ref this.package, value); }
        }

        public ExternalComponent[] Components => ProductInformation.Components;
        public string Copyright => ProductInformation.Copyright;
        public string DonateUrl => ContactInformation.PayPalLink;
        public string WebsiteLink => ContactInformation.WebsiteLink;
        public string WebsiteContactLink => ContactInformation.WebsiteContactLink;
        public string FacebookLink => ContactInformation.FacebookLink;
        public string TwitterLink => ContactInformation.TwitterLink;

        public InformationAboutViewModel(IUnityContainer container, IDialogService dialogService)
        {
            this.container = container;
            this.dialogService = dialogService;
            this.Package = new Package(ProcessExecutable.Name(), ProcessExecutable.AssemblyVersion(), Configuration.Release);

            this.ShowLicenseCommand = new DelegateCommand(() =>
            {
                var view = this.container.Resolve<InformationAboutLicense>();

                this.dialogService.ShowCustomDialog(
                    0xe73e,
                    16,
                    ResourceUtils.GetString("Language_License"),
                    view,
                    400,
                    0,
                    false,
                    true,
                    true,
                    false,
                    ResourceUtils.GetString("Language_Ok"),
                    string.Empty,
                    null);
            });

            this.OpenLinkCommand = new DelegateCommand<string>((url) =>
            {
                try
                {
                    Actions.TryOpenLink(url);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open link {0}. Exception: {1}", url, ex.Message);
                }
            });
        }
    }
}
