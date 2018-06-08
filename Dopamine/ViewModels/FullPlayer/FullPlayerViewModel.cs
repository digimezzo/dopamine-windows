using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Dopamine.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace Dopamine.ViewModels.FullPlayer
{
    public class FullPlayerViewModel : BindableBase
    {
        private IRegionManager regionManager;
        private FullPlayerPage previousSelectedFullPlayerPage;
        private IIndexingService indexingService;
        private int slideInFrom;

        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand<string> SetSelectedFullPlayerPageCommand { get; set; }

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public FullPlayerViewModel(IIndexingService indexingService, IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.indexingService = indexingService;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage(FullPlayerPage.Collection));
            this.SetSelectedFullPlayerPageCommand = new DelegateCommand<string>(pageIndex => this.NagivateToSelectedPage((FullPlayerPage) Int32.Parse(pageIndex)));
        }

        private void NagivateToSelectedPage(FullPlayerPage page)
        {
            this.SlideInFrom = page <= this.previousSelectedFullPlayerPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedFullPlayerPage = page;

            switch (page)
            {
                case FullPlayerPage.Collection:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Collection.Collection).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Collection.CollectionMenu).FullName);
                    break;
                case FullPlayerPage.Settings:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Settings.Settings).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Settings.SettingsMenu).FullName);
                    break;
                case FullPlayerPage.Information:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Information.Information).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Information.InformationMenu).FullName);
                    break;
                default:
                    break;
            }

            if (page != FullPlayerPage.Settings)
            {
                this.indexingService.RefreshCollectionIfFoldersChangedAsync();
            }
        }
    }
}
