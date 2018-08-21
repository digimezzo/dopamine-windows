using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Services.Collection;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Folders;
using Dopamine.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WPFFolderBrowser;

namespace Dopamine.ViewModels.Common
{
    public class CollectionFoldersSettingsViewModel : BindableBase
    {
        private IIndexingService indexingService;
        private IDialogService dialogService;
        private ICollectionService collectionservice;
        private IFoldersService foldersService;
        private ObservableCollection<FolderViewModel> folders;
        private bool isLoadingFolders;
        private bool showAllFoldersInCollection;
        private bool isIndexing;

        public DelegateCommand<string> AddFolderCommand { get; set; }

        public DelegateCommand<long?> RemoveFolderCommand { get; set; }

        public DelegateCommand<long?> ShowInCollectionChangedCommand { get; set; }

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
                RaisePropertyChanged(nameof(this.IsBusy));
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
                RaisePropertyChanged(nameof(this.IsBusy));
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

        public CollectionFoldersSettingsViewModel(IIndexingService indexingService, IDialogService dialogService, 
            ICollectionService collectionservice, IFoldersService foldersService)
        {
            this.indexingService = indexingService;
            this.dialogService = dialogService;
            this.collectionservice = collectionservice;
            this.foldersService = foldersService;

            this.AddFolderCommand = new DelegateCommand<string>((_) => { this.AddFolder(); });

            this.RemoveFolderCommand = new DelegateCommand<long?>(folderId =>
            {
                if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetString("Language_Remove"), ResourceUtils.GetString("Language_Confirm_Remove_Folder"), ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
                {
                    this.RemoveFolder(folderId.Value);
                }
            });

            this.ShowInCollectionChangedCommand = new DelegateCommand<long?>(folderId =>
            {
                this.ShowAllFoldersInCollection = false;

                lock (this.Folders)
                {
                    this.foldersService.MarkFolderAsync(this.Folders.Where((f) => f.Folder.FolderID == folderId).FirstOrDefault());
                }
            });

            this.ShowAllFoldersInCollection = SettingsClient.Get<bool>("Indexing", "ShowAllFoldersInCollection");

            // Makes sure IsIndexng is set if this ViewModel is created after the indexer has started indexing
            if (this.indexingService.IsIndexing)
            {
                this.IsIndexing = true;
            }

            // These events handle changes of indexer status after the ViewModel is created
            this.indexingService.IndexingStarted += (_, __) => this.IsIndexing = true;
            this.indexingService.IndexingStopped += (_, __) => this.IsIndexing = false;

            this.GetFoldersAsync();
        }

        private async void AddFolder()
        {
            LogClient.Info("Adding a folder to the collection.");

            var dlg = new WPFFolderBrowserDialog();

            if ((bool)dlg.ShowDialog())
            {
                try
                {
                    this.IsLoadingFolders = true;

                    AddFolderResult result = AddFolderResult.Error; // Initial value

                    // First, check if the folder's content is accessible. If not, don't add the folder.
                    if (FileOperations.IsDirectoryContentAccessible(dlg.FileName))
                    {
                        result = await this.foldersService.AddFolderAsync(dlg.FileName);
                    }
                    else
                    {
                        result = AddFolderResult.Inaccessible;
                    }

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
                        case AddFolderResult.Inaccessible:

                            this.dialogService.ShowNotification(
                                0xe711,
                                16,
                                ResourceUtils.GetString("Language_Error"),
                                ResourceUtils.GetString("Language_Folder_Could_Not_Be_Added_Check_Permissions").Replace("{foldername}", dlg.FileName),
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

        private async void RemoveFolder(long folderId)
        {
            try
            {
                this.IsLoadingFolders = true;
                RemoveFolderResult result = await this.foldersService.RemoveFolderAsync(folderId);
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

            var localFolders = new ObservableCollection<FolderViewModel>(await this.foldersService.GetFoldersAsync());

            this.IsLoadingFolders = false;

            this.Folders = localFolders;
        }

        private async void ForceShowAllFoldersInCollection()
        {
            if (this.Folders == null) return;

            await Task.Run(() =>
            {
                lock (this.Folders)
                {
                    foreach (FolderViewModel folder in this.Folders)
                    {
                        folder.ShowInCollection = true;
                        this.foldersService.MarkFolderAsync(folder);
                    }
                }
            });
        }
    }
}
