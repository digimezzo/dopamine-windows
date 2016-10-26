using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Indexing;
using Dopamine.Core.Settings;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading.Tasks;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsCollectionViewModel : BindableBase, IActiveAware, INavigationAware
    {
        #region Variables
        private bool isActive;
        private bool checkBoxIgnoreRemovedFilesChecked;
        private bool checkBoxRefreshCollectionOnStartupChecked;
        private IIndexingService indexingService;
        private ICollectionService collectionService;
        #endregion

        #region Commands
        public DelegateCommand RefreshNowCommand { get; set; }
        #endregion

        #region Properties
        public bool IsActive
        {
            get { return this.isActive; }
            set { SetProperty<bool>(ref this.isActive, value); }
        }

        public bool CheckBoxIgnoreRemovedFilesChecked
        {
            get { return this.checkBoxIgnoreRemovedFilesChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Indexing", "IgnoreRemovedFiles", value);
                SetProperty<bool>(ref this.checkBoxIgnoreRemovedFilesChecked, value);
            }
        }

        public bool CheckBoxRefreshCollectionOnStartupChecked
        {
            get { return this.checkBoxRefreshCollectionOnStartupChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Indexing", "RefreshCollectionOnStartup", value);
                SetProperty<bool>(ref this.checkBoxRefreshCollectionOnStartupChecked, value);
            }
        }
        #endregion

        #region Construction
        public SettingsCollectionViewModel(IIndexingService indexingService, ICollectionService collectionService)
        {
            this.indexingService = indexingService;
            this.collectionService = collectionService;

            this.RefreshNowCommand = new DelegateCommand(() => this.RefreshNow());

            this.GetCheckBoxesAsync();
        }
        #endregion

        #region IActiveAware
        public event EventHandler IsActiveChanged;
        #endregion

        #region Private
        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxIgnoreRemovedFilesChecked = XmlSettingsClient.Instance.Get<bool>("Indexing", "IgnoreRemovedFiles");
                this.CheckBoxRefreshCollectionOnStartupChecked = XmlSettingsClient.Instance.Get<bool>("Indexing", "RefreshCollectionOnStartup");
            });
        }

        private void RefreshNow()
        {
            this.indexingService.NeedsIndexing = true;
            this.indexingService.IndexCollectionAsync(XmlSettingsClient.Instance.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.indexingService.DelayedIndexCollectionAsync(1000, XmlSettingsClient.Instance.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
            this.collectionService.SaveMarkedFoldersAsync();
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }
        #endregion

    }
}
