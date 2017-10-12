using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Extensions;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WPFFolderBrowser;

namespace Dopamine.ViewModels.Common
{
    public class CollectionFoldersViewModel : BindableBase
    {
        #region Variables
        private IIndexingService indexingService;
        private IDialogService dialogService;
        private ICollectionService collectionservice;
        private IFolderRepository folderRepository;
        private ObservableCollection<FolderViewModel> folders;
        private bool isLoadingFolders;
        private bool showAllFoldersInCollection;
        private bool isIndexing;
        #endregion

        #region Commands
        public DelegateCommand<string> AddFolderCommand { get; set; }
        public DelegateCommand<string> RemoveFolderCommand { get; set; }
        public DelegateCommand<string> ShowInCollectionChangedCommand { get; set; }
        #endregion

        #region Properties
        public bool IsBusy
        {
            get { return this.IsIndexing | this.IsLoadingFolders; }
        }

        public bool IsIndexing
        {
            get { return this.isIndexing; }
            set
            {
                SetProperty<bool>(ref this.isIndexing, value);
                OnPropertyChanged(() => this.IsBusy);
            }
        }

        public ObservableCollection<FolderViewModel> Folders
        {
            get { return this.folders; }
            set { SetProperty<ObservableCollection<FolderViewModel>>(ref this.folders, value); }
        }

        public bool IsLoadingFolders
        {
            get { return this.isLoadingFolders; }
            set
            {
                SetProperty<bool>(ref this.isLoadingFolders, value);
                OnPropertyChanged(() => this.IsBusy);
            }
        }

        public bool ShowAllFoldersInCollection
        {
            get { return this.showAllFoldersInCollection; }
            set
            {
                SetProperty<bool>(ref this.showAllFoldersInCollection, value);

                if (value)
                {
                    this.ForceShowAllFoldersInCollection();
                }

                SettingsClient.Set<bool>("Indexing", "ShowAllFoldersInCollection", value);
            }
        }
        #endregion

        #region Construction
        public CollectionFoldersViewModel(IIndexingService indexingService, IDialogService dialogService, ICollectionService collectionservice, IFolderRepository folderRepository)
        {
            this.indexingService = indexingService;
            this.dialogService = dialogService;
            this.collectionservice = collectionservice;
            this.folderRepository = folderRepository;

            this.AddFolderCommand = new DelegateCommand<string>((_) => { this.AddFolder(); });

            this.RemoveFolderCommand = new DelegateCommand<string>(iPath =>
            {
                if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetString("Language_Remove"), ResourceUtils.GetString("Language_Confirm_Remove_Folder"), ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
                {
                    this.RemoveFolder(iPath);
                }
            });

            this.ShowInCollectionChangedCommand = new DelegateCommand<string>(path =>
            {
                this.ShowAllFoldersInCollection = false;

                lock (this.Folders)
                {
                    this.collectionservice.MarkFolderAsync(this.Folders.Select((f) => f.Folder).Where((f) => f.SafePath.Equals(path.ToSafePath())).FirstOrDefault());
                }
            });

            this.ShowAllFoldersInCollection = SettingsClient.Get<bool>("Indexing", "ShowAllFoldersInCollection");

            // Makes sure Me.IsIndexng is set if this ViewModel is created after the Indexer has started indexing
            if (this.indexingService.IsIndexing)
            {
                this.IsIndexing = true;
            }

            // These events handle changes of Indexer status after the ViewModel is created
            this.indexingService.IndexingStarted += (_, __) => this.IsIndexing = true;
            this.indexingService.IndexingStopped += (_, __) => this.IsIndexing = false;

            this.GetFoldersAsync();
        }
        #endregion

        #region Private
        private async void AddFolder()
        {
            LogClient.Info("Adding a folder to the collection.");

            var dlg = new WPFFolderBrowserDialog();

            if ((bool)dlg.ShowDialog())
            {
                try
                {
                    this.IsLoadingFolders = true;
                    AddFolderResult result = await this.folderRepository.AddFolderAsync(dlg.FileName);
                    this.IsLoadingFolders = false;

                    switch (result)
                    {
                        case AddFolderResult.Success:
                            this.indexingService.OnFoldersChanged();
                            this.GetFoldersAsync();
                            break;
                        case AddFolderResult.Error:
                            this.dialogService.ShowNotification(
                                0xe711,
                                16,
                                ResourceUtils.GetString("Language_Error"),
                                ResourceUtils.GetString("Language_Error_Adding_Folder"),
                                ResourceUtils.GetString("Language_Ok"),
                                true,
                                ResourceUtils.GetString("Language_Log_File"));
                            break;
                        case AddFolderResult.Duplicate:

                            this.dialogService.ShowNotification(
                                0xe711,
                                16,
                                ResourceUtils.GetString("Language_Already_Exists"),
                                ResourceUtils.GetString("Language_Folder_Already_In_Collection"),
                                ResourceUtils.GetString("Language_Ok"),
                                false,
                                "");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Exception: {0}", ex.Message);

                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Folder"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                }
                finally
                {
                    this.IsLoadingFolders = false;
                }
            }
        }

        private async void RemoveFolder(string path)
        {
            try
            {
                this.IsLoadingFolders = true;
                RemoveFolderResult result = await this.folderRepository.RemoveFolderAsync(path);
                this.IsLoadingFolders = false;

                switch (result)
                {
                    case RemoveFolderResult.Success:
                        this.indexingService.OnFoldersChanged();
                        this.GetFoldersAsync();
                        break;
                    case RemoveFolderResult.Error:
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Removing_Folder"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                        break;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Exception: {0}", ex.Message);

                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Removing_Folder"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
            finally
            {
                this.IsLoadingFolders = false;
            }
        }

        private async void GetFoldersAsync()
        {
            this.IsLoadingFolders = true;

            List<Folder> foldersList = await this.folderRepository.GetFoldersAsync();

            var localFolders = new ObservableCollection<FolderViewModel>();

            await Task.Run(() =>
            {
                foreach (Folder fol in foldersList)
                {
                    localFolders.Add(new FolderViewModel { Folder = fol });
                }

            });

            this.IsLoadingFolders = false;

            this.Folders = localFolders;
        }

        private async void ForceShowAllFoldersInCollection()
        {
            if (this.Folders == null) return;

            await Task.Run(() => {
                lock (this.Folders)
                {
                    foreach (FolderViewModel fol in this.Folders)
                    {
                        fol.ShowInCollection = true;
                        this.collectionservice.MarkFolderAsync(fol.Folder);
                    }
                }
            });
        }
        #endregion
    }
}
