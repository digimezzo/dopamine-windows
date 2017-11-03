using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Indexing;
using Dopamine.Views.FullPlayer.Settings;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsViewModel : BindableBase
    {
        private SettingsPage previousSelectedSettingsPage;
        private SettingsPage selectedSettingsPage;
        private IIndexingService indexingService;
        private IRegionManager regionManager;
        private int slideInFrom;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public SettingsPage SelectedSettingsPage
        {
            get { return selectedSettingsPage; }
            set {
                SetProperty<SettingsPage>(ref this.selectedSettingsPage, value);
                this.NagivateToSelectedPage();

                if (value != SettingsPage.Collection)
                {
                    this.indexingService.RefreshCollectionIfFoldersChangedAsync();
                }
            }
        }

        public DelegateCommand LoadedCommand { get; set; }

        public SettingsViewModel(IIndexingService indexingService, IRegionManager regionManager)
        {
            this.indexingService = indexingService;
            this.regionManager = regionManager;

            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedSettingsPage <= this.previousSelectedSettingsPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedSettingsPage = this.selectedSettingsPage;

            switch (this.selectedSettingsPage)
            {
                case SettingsPage.Appearance:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsAppearance).FullName);
                    break;
                case SettingsPage.Behaviour:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsBehaviour).FullName);
                    break;
                case SettingsPage.Collection:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsCollection).FullName);
                    break;
                case SettingsPage.Online:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsOnline).FullName);
                    break;
                case SettingsPage.Playback:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsPlayback).FullName);
                    break;
                case SettingsPage.Startup:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsStartup).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
