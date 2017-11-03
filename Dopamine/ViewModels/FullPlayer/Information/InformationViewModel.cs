using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Dopamine.Views.FullPlayer.Information;
using Prism.Commands;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationViewModel : NavigationViewModelBase
    {
        private InformationPage previousSelectedInformationPage;
        private InformationPage selectedInformationPage;

        public InformationPage SelectedInformationPage
        {
            get { return selectedInformationPage; }
            set
            {
                SetProperty<InformationPage>(ref this.selectedInformationPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public InformationViewModel(IRegionManager regionManager) : base(regionManager)
        {
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedInformationPage <= this.previousSelectedInformationPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedInformationPage = this.selectedInformationPage;

            switch (this.selectedInformationPage)
            {
                case InformationPage.Help:
                    this.RegionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationHelp).FullName);
                    break;
                case InformationPage.About:
                    this.RegionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationAbout).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
