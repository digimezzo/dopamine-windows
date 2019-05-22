using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Dopamine.Services.Dialog;
using Dopamine.Services.Folders;
using Dopamine.Services.Indexing;
using Dopamine.Views.FullPlayer;
using Prism.Commands;
using Prism.Ioc;
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
        private IContainerProvider container;
        private IDialogService dialogService;
        private IFoldersService foldersService;
        private int slideInFrom;
        private bool showBackButton;

        public DelegateCommand LoadedCommand { get; set; }

        public DelegateCommand AddMusicCommand { get; set; }

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

        public FullPlayerViewModel(IIndexingService indexingService, IRegionManager regionManager,
            IContainerProvider container, IDialogService dialogService, IFoldersService foldersService)
        {
            this.regionManager = regionManager;
            this.indexingService = indexingService;
            this.container = container;
            this.dialogService = dialogService;
            this.foldersService = foldersService;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage(FullPlayerPage.Collection));
            this.SetSelectedFullPlayerPageCommand = new DelegateCommand<string>(pageIndex => this.NagivateToSelectedPage((FullPlayerPage)Int32.Parse(pageIndex)));
            this.BackButtonCommand = new DelegateCommand(() => this.NagivateToSelectedPage(FullPlayerPage.Collection));
            this.AddMusicCommand = new DelegateCommand(() => this.AddMusicAsync());
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
                    this.ShowBackButton = false;
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
        }

        private async void AddMusicAsync()
        {
            FullPlayerAddMusic view = this.container.Resolve<FullPlayerAddMusic>();
            view.DataContext = this.container.Resolve<FullPlayerAddMusicViewModel>();

            this.dialogService.ShowCustomDialog(
                0xE8D6,
                16,
                ResourceUtils.GetString("Language_Add_Music"),
                view,
                500,
                400,
                false,
                false,
                false,
                false,
                ResourceUtils.GetString("Language_Ok"),
                string.Empty,
                null);

            await this.foldersService.SaveToggledFoldersAsync();
            this.indexingService.RefreshCollectionIfFoldersChangedAsync();
        }
    }
}
