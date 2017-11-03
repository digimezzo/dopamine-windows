using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Views.FullPlayer.Information;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationViewModel : BindableBase
    {
        private InformationPage previousSelectedInformationPage;
        private InformationPage selectedInformationPage;
        private IRegionManager regionManager;
        private int slideInFrom;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public DelegateCommand LoadedCommand { get; set; }

        public InformationPage SelectedInformationPage
        {
            get { return selectedInformationPage; }
            set
            {
                SetProperty<InformationPage>(ref this.selectedInformationPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public InformationViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedInformationPage <= this.previousSelectedInformationPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedInformationPage = this.selectedInformationPage;

            switch (this.selectedInformationPage)
            {
                case InformationPage.Help:
                    this.regionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationHelp).FullName);
                    break;
                case InformationPage.About:
                    this.regionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationAbout).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
