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
        private FullPlayerPage previousSelectedPage;
        private FullPlayerPage goBackPage;
        private IIndexingService indexingService;
        private int slideInFrom;
        private bool showBackButton;

        public DelegateCommand LoadedCommand { get; set; }

        public DelegateCommand<string> SetSelectedFullPlayerPageCommand { get; set; }

        public DelegateCommand BackButtonCommand { get; set; }

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public bool ShowBackButton
        {
            get { return this.showBackButton; }
            set { SetProperty<bool>(ref this.showBackButton, value); }
        }

        public FullPlayerViewModel(IIndexingService indexingService, IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.indexingService = indexingService;
            this.goBackPage = FullPlayerPage.Collection;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage(FullPlayerPage.Collection));
            this.SetSelectedFullPlayerPageCommand = new DelegateCommand<string>(pageIndex => this.NagivateToSelectedPage((FullPlayerPage) Int32.Parse(pageIndex)));
            this.BackButtonCommand = new DelegateCommand(() => this.NagivateToSelectedPage(this.goBackPage));
        }

        private void NagivateToSelectedPage(FullPlayerPage page)
        {
            this.SlideInFrom = page <= this.previousSelectedPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedPage = page;

            switch (page)
            {
                case FullPlayerPage.Collection:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Collection.Collection).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Collection.CollectionMenu).FullName);
                    this.ShowBackButton = false;
                    this.goBackPage = FullPlayerPage.Collection;
                    break;
                case FullPlayerPage.Playlists:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Playlists.Playlists).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Playlists.PlaylistsMenu).FullName);
                    this.ShowBackButton = false;
                    this.goBackPage = FullPlayerPage.Playlists;
                    break;
                case FullPlayerPage.Settings:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Settings.Settings).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Settings.SettingsMenu).FullName);
                    this.ShowBackButton = true;
                    break;
                case FullPlayerPage.Information:
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerRegion, typeof(Views.FullPlayer.Information.Information).FullName);
                    this.regionManager.RequestNavigate(RegionNames.FullPlayerMenuRegion, typeof(Views.FullPlayer.Information.InformationMenu).FullName);
                    this.ShowBackButton = true;
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
