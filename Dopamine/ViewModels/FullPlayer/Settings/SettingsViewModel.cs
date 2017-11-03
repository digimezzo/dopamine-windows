using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Indexing;
using Dopamine.Views.FullPlayer.Settings;
using Prism.Commands;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsViewModel : NavigationViewModelBase
    {
        private SettingsPage previousSelectedSettingsPage;
        private SettingsPage selectedSettingsPage;
        private IIndexingService indexingService;

        public SettingsPage SelectedSettingsPage
        {
            get { return selectedSettingsPage; }
            set
            {
                SetProperty<SettingsPage>(ref this.selectedSettingsPage, value);
                this.NagivateToSelectedPage();

                if (value != SettingsPage.Collection)
                {
                    this.indexingService.RefreshCollectionIfFoldersChangedAsync();
                }
            }
        }

        public SettingsViewModel(IIndexingService indexingService, IRegionManager regionManager) : base(regionManager)
        {
            this.indexingService = indexingService;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.SlideInFrom = this.selectedSettingsPage <= this.previousSelectedSettingsPage ? -Constants.SlideDistance : Constants.SlideDistance;
            this.previousSelectedSettingsPage = this.selectedSettingsPage;

            switch (this.selectedSettingsPage)
            {
                case SettingsPage.Appearance:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsAppearance).FullName);
                    break;
                case SettingsPage.Behaviour:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsBehaviour).FullName);
                    break;
                case SettingsPage.Collection:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsCollection).FullName);
                    break;
                case SettingsPage.Online:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsOnline).FullName);
                    break;
                case SettingsPage.Playback:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsPlayback).FullName);
                    break;
                case SettingsPage.Startup:
                    this.RegionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsStartup).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
