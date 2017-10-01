using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Prism;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsViewModel : BindableBase, IActiveAware, INavigationAware
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IIndexingService indexingService;
        private ICollectionService collectionService;
        private int previousIndex = 0;
        private int slideInFrom;
        #endregion

        #region Commands
        public DelegateCommand<string> NavigateBetweenSettingsCommand;
        #endregion

        #region Properties
        public bool IsActive { get; set; }

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }
        #endregion

        #region Construction
        public SettingsViewModel(IRegionManager regionManager, IIndexingService indexingService, ICollectionService collectionService)
        {
            this.regionManager = regionManager;
            this.indexingService = indexingService;
            this.collectionService = collectionService;
            this.NavigateBetweenSettingsCommand = new DelegateCommand<string>(NavigateBetweenSettings);
            ApplicationCommands.NavigateBetweenSettingsCommand.RegisterCommand(this.NavigateBetweenSettingsCommand);
            this.SlideInFrom = 30;
        }
        #endregion

        #region IActiveAware
        public event EventHandler IsActiveChanged;
        #endregion

        #region Private
        private void NavigateBetweenSettings(string iIndex)
        {
            if (string.IsNullOrWhiteSpace(iIndex))
                return;

            int index = 0;

            int.TryParse(iIndex, out index);

            if (index == 0)
                return;

            this.SlideInFrom = index <= this.previousIndex ? -30 : 30;

            this.previousIndex = index;

            this.regionManager.RequestNavigate(RegionNames.SettingsRegion, this.GetPageForIndex(index));
        }

        private string GetPageForIndex(int iIndex)
        {

            string page = string.Empty;

            switch (iIndex)
            {
                case 1:
                    page = typeof(Views.SettingsCollection).FullName;
                    break;
                case 2:
                    page = typeof(Views.SettingsAppearance).FullName;
                    break;
                case 3:
                    page = typeof(Views.SettingsBehaviour).FullName;
                    break;
                case 4:
                    page = typeof(Views.SettingsPlayback).FullName;
                    break;
                case 5:
                    page = typeof(Views.SettingsStartup).FullName;
                    break;
                case 6:
                    page = typeof(Views.SettingsOnline).FullName;
                    break;
                default:
                    page = typeof(Views.SettingsCollection).FullName;
                    break;
            }

            return page;
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.indexingService.CheckCollectionAsync();
            this.collectionService.SaveMarkedFoldersAsync();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }
        #endregion
    }
}
