using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer
{
    public class FullPlayerViewModel : BindableBase
    {
        private FullPlayerPage previousSelectedFullPlayerPage;
        private FullPlayerPage selectedFullPlayerPage;
        private IIndexingService indexingService;
        private IRegionManager regionManager;
        private int slideInFrom;

        public DelegateCommand LoadedCommand { get; set; }

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public FullPlayerPage SelectedFullPlayerPage
        {
            get { return selectedFullPlayerPage; }
            set
            {
                SetProperty<FullPlayerPage>(ref this.selectedFullPlayerPage, value);
                this.NagivateToSelectedPage();

                if (value != FullPlayerPage.Settings)
                {
                    this.indexingService.RefreshCollectionIfFoldersChangedAsync();
                }
            }
        }

        public FullPlayerViewModel(IIndexingService indexingService, IRegionManager regionManager)
        {
            this.indexingService = indexingService;
            this.regionManager = regionManager;

            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedFullPlayerPage <= this.previousSelectedFullPlayerPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedFullPlayerPage = this.selectedFullPlayerPage;

            switch (this.selectedFullPlayerPage)
            {
                case FullPlayerPage.Collection:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Collection.Collection).FullName);
                    break;
                case FullPlayerPage.Settings:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Settings.Settings).FullName);
                    break;
                case FullPlayerPage.Information:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Information.Information).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
