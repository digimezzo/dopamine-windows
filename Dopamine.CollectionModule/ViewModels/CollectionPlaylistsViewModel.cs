using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.CollectionModule.Views;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Prism;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFFolderBrowser;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionPlaylistsViewModel : CommonTracksViewModel, IDropTarget
    {
        #region Variables
        // Lists
        private ObservableCollection<PlaylistViewModel> playlists;
        private IList<Playlist> selectedPlaylists;

        // Flags
        private bool isLoadingPlaylists;

        // Repositories
        private IPlaylistRepository playlistRepository;

        // Other
        private long playlistsCount;
        private double leftPaneWidthPercent;
        #endregion

        #region Commands
        public DelegateCommand NewPlaylistCommand { get; set; }
        public DelegateCommand OpenPlaylistCommand { get; set; }
        public DelegateCommand<string> DeletePlaylistByNameCommand { get; set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }
        public DelegateCommand DeleteSelectedPlaylistsCommand { get; set; }
        public DelegateCommand SaveSelectedPlaylistsCommand { get; set; }
        public DelegateCommand<object> SelectedPlaylistsCommand { get; set; }
        public DelegateCommand AddPlaylistsToNowPlayingCommand { get; set; }
        #endregion

        #region Properties
        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public bool AllowRename
        {
            get
            {
                if (this.SelectedPlaylists != null)
                {
                    return this.SelectedPlaylists.Count == 1;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsLoadingPlaylists
        {
            get { return this.isLoadingPlaylists; }
            set { SetProperty<bool>(ref this.isLoadingPlaylists, value); }
        }

        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return this.playlists; }
            set { SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.playlists, value); }
        }

        public IList<Playlist> SelectedPlaylists
        {
            get { return this.selectedPlaylists; }
            set { SetProperty<IList<Playlist>>(ref this.selectedPlaylists, value); }
        }

        public long PlaylistsCount
        {
            get { return this.playlistsCount; }
            set { SetProperty<long>(ref this.playlistsCount, value); }
        }

        public override bool CanOrderByAlbum
        {
            // Doesn't need to return a useful value in this class
            get { return false; }
        }
        #endregion

        #region Construction
        public CollectionPlaylistsViewModel(IUnityContainer container, IPlaylistRepository playlistRepository) : base(container)
        {
            // Repositories
            this.playlistRepository = playlistRepository;

            // Commands
            this.NewPlaylistCommand = new DelegateCommand(async () => await this.ConfirmAddPlaylistAsync());
            this.OpenPlaylistCommand = new DelegateCommand(async () => await this.OpenPlaylistAsync());
            this.DeletePlaylistByNameCommand = new DelegateCommand<string>(async (iPlaylistName) => await this.DeletePlaylistByNameAsync(iPlaylistName));
            this.DeleteSelectedPlaylistsCommand = new DelegateCommand(async () => await this.DeleteSelectedPlaylistsAsync());
            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());
            this.SaveSelectedPlaylistsCommand = new DelegateCommand(async () => await this.SaveSelectedPlaylistsAsync());
            this.SelectedPlaylistsCommand = new DelegateCommand<object>(async (parameter) => await SelectedPlaylistsHandlerAsync(parameter));
            this.AddPlaylistsToNowPlayingCommand = new DelegateCommand(async () => await this.AddPLaylistsToNowPlayingAsync(this.SelectedPlaylists));

            // Events
            this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
            {
                if (screenName == typeof(CollectionPlaylists).FullName) this.RemoveSelectedTracksCommand.Execute();
            });

            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(enableRating => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe(enableLove => this.EnableLove = enableLove);

            // MetadataService
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;

            // CollectionService
            this.collectionService.AddedTracksToPlaylist += async (_, __) => await this.ReloadPlaylistsAsync();
            this.collectionService.DeletedTracksFromPlaylists += async (_, __) => await this.ReloadPlaylistsAsync();
            this.collectionService.PlaylistsChanged += async (_, __) => await this.FillListsAsync(); // Refreshes the lists when the playlists have changed

            // Events
            this.eventAggregator.GetEvent<RenameSelectedPlaylistWithKeyF2>().Subscribe(async (_) => await this.RenameSelectedPlaylistAsync());
            this.eventAggregator.GetEvent<DeleteSelectedPlaylistsWithKeyDelete>().Subscribe(async (_) => await this.DeleteSelectedPlaylistsAsync());

            this.TrackOrder = TrackOrder.None;

            // Subscribe to Events and Commands on creation
            this.Subscribe();

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }
        #endregion

        #region Private
        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsAlbumChanged | e.IsTrackChanged) await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
        }

        protected async Task AddPLaylistsToNowPlayingAsync(IList<Playlist> playlists)
        {
            AddToQueueResult result = await this.playbackService.AddToQueue(playlists);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task GetPlaylistsAsync()
        {
            try
            {
                // Notify the user
                this.IsLoadingPlaylists = true;

                // Get the Albums from the database
                IList<Playlist> playlists = await this.playlistRepository.GetPlaylistsAsync();

                // Set the count
                this.PlaylistsCount = playlists.Count;

                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>();

                await Task.Run(() =>
                {
                    foreach (Playlist pl in playlists)
                    {
                        playlistViewModels.Add(new PlaylistViewModel { Playlist = pl });
                    }
                });

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occured while getting Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }
            finally
            {
                // Stop notifying
                this.IsLoadingPlaylists = false;
            }
        }

        public async Task GetTracksAsync(IList<Playlist> selectedPlaylists, TrackOrder trackOrder)
        {
            await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(selectedPlaylists), trackOrder);
        }

        private async Task ConfirmAddPlaylistAsync()
        {
            string responseText = ResourceUtils.GetStringResource("Language_New_Playlist");

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetStringResource("Language_New_Playlist"),
                ResourceUtils.GetStringResource("Language_Enter_Name_For_New_Playlist"),
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ref responseText))
            {
                await this.AddPlaylistAsync(responseText);
            }
        }

        private async Task OpenPlaylistAsync()
        {
            // Set up the file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = Application.Current.FindResource("Language_Open_Playlist").ToString();
            dlg.DefaultExt = FileFormats.M3U; // Default file extension

            // Filter files by extension
            dlg.Filter = ResourceUtils.GetStringResource("Language_Playlists") + " (*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL + ")|*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL;

            // Show the file dialog box
            bool? dialogResult = dlg.ShowDialog();

            // Process the file dialog box result

            if ((bool)dialogResult)
            {
                this.IsLoadingPlaylists = true;

                OpenPlaylistResult openResult = await this.collectionService.OpenPlaylistAsync(dlg.FileName);

                if (openResult == OpenPlaylistResult.Error)
                {
                    this.IsLoadingPlaylists = false;

                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Opening_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                }
            }
        }

        private async Task AddPlaylistAsync(string playlistName)
        {
            this.IsLoadingPlaylists = true;

            AddPlaylistResult result = await this.collectionService.AddPlaylistAsync(playlistName);

            switch (result)
            {
                case AddPlaylistResult.Success:
                    await this.FillListsAsync();
                    break;
                case AddPlaylistResult.Duplicate:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Already_Exists"),
                        ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", "\"" + playlistName + "\""),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case AddPlaylistResult.Error:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                    break;
                case AddPlaylistResult.Blank:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    this.IsLoadingPlaylists = false;
                    break;
            }
        }

        private async Task DeletePlaylistByNameAsync(string playlistName)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetStringResource("Language_Delete"),
                ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", "\"" + playlistName + "\""),
                ResourceUtils.GetStringResource("Language_Yes"),
                ResourceUtils.GetStringResource("Language_No")))
            {
                List<Playlist> playlists = new List<Playlist>();
                playlists.Add(new Playlist { PlaylistName = playlistName });

                await this.DeletePlaylistsAsync(playlists);
            }
        }

        private async Task DeleteSelectedPlaylistsAsync()
        {

            if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
            {
                string title = ResourceUtils.GetStringResource("Language_Delete");
                string message = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlists");

                if (this.SelectedPlaylists.Count == 1)
                {
                    message = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", "\"" + this.SelectedPlaylists[0].PlaylistName + "\"");
                }

                if (this.dialogService.ShowConfirmation(
                    0xe11b,
                    16,
                    title,
                    message,
                    ResourceUtils.GetStringResource("Language_Yes"),
                    ResourceUtils.GetStringResource("Language_No")))
                {
                    await this.DeletePlaylistsAsync(this.SelectedPlaylists);
                }
            }
        }

        private async Task DeletePlaylistsAsync(IList<Playlist> playlists)
        {
            this.IsLoadingPlaylists = true;
            DeletePlaylistResult result = await this.collectionService.DeletePlaylistsAsync(playlists);

            await this.FillListsAsync();

            if (result == DeletePlaylistResult.Error)
            {
                string message = ResourceUtils.GetStringResource("Language_Error_Deleting_Playlists");

                if (playlists.Count == 1)
                {
                    message = ResourceUtils.GetStringResource("Language_Error_Deleting_Playlist").Replace("%playlistname%", "\"" + playlists[0].PlaylistName + "\"");
                }

                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetStringResource("Language_Error"),
                    message,
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
            {
                string oldPlaylistName = this.SelectedPlaylists[0].PlaylistName;
                string responseText = oldPlaylistName;

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetStringResource("Language_Rename_Playlist"),
                    ResourceUtils.GetStringResource("Language_Enter_New_Name_For_Playlist").Replace("%playlistname%", oldPlaylistName),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    ResourceUtils.GetStringResource("Language_Cancel"),
                    ref responseText))
                {
                    await this.RenamePlaylistAsync(oldPlaylistName, responseText);
                }
            }
        }

        private async Task RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            this.IsLoadingPlaylists = true;
            RenamePlaylistResult result = await this.collectionService.RenamePlaylistAsync(oldPlaylistName, newPlaylistName);

            switch (result)
            {
                case RenamePlaylistResult.Success:
                    await this.FillListsAsync();
                    break;
                case RenamePlaylistResult.Duplicate:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Already_Exists"),
                        ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", "\"" + newPlaylistName + "\""),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case RenamePlaylistResult.Error:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Renaming_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                    break;
                case RenamePlaylistResult.Blank:
                    this.IsLoadingPlaylists = false;
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    this.IsLoadingPlaylists = false;
                    break;
            }
        }

        private async Task DeleteTracksFromPlaylistsAsync()
        {
            DeleteTracksFromPlaylistsResult result = await this.collectionService.DeleteTracksFromPlaylistAsync(this.SelectedTracks, this.SelectedPlaylists.FirstOrDefault());

            switch (result)
            {
                case DeleteTracksFromPlaylistsResult.Success:
                    await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
                    break;
                case DeleteTracksFromPlaylistsResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Removing_From_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                    break;
                default:
                    break;
                    // Never happens
            }
        }

        private async Task ReloadPlaylistsAsync()
        {
            if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
            {
                await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
            }
        }

        private async Task SelectedPlaylistsHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedPlaylists = new List<Playlist>();

                foreach (PlaylistViewModel item in (IList)parameter)
                {
                    this.SelectedPlaylists.Add(item.Playlist);
                }
                OnPropertyChanged(() => this.AllowRename);
            }

            // Don't reload the lists when updating Metadata. MetadataChangedHandlerAsync handles that.
            if (this.metadataService.IsUpdatingDatabaseMetadata) return;

            await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
        }

        private async Task SaveSelectedPlaylistsAsync()
        {
            if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
            {
                if (this.SelectedPlaylists.Count > 1)
                {
                    // Save all the selected playlists
                    // -------------------------------
                    var dlg = new WPFFolderBrowserDialog();

                    if ((bool)dlg.ShowDialog())
                    {
                        try
                        {
                            ExportPlaylistsResult result = await this.collectionService.ExportPlaylistsAsync(this.SelectedPlaylists, dlg.FileName);

                            if (result == ExportPlaylistsResult.Error)
                            {
                                this.dialogService.ShowNotification(
                                        0xe711,
                                        16,
                                        ResourceUtils.GetStringResource("Language_Error"),
                                        ResourceUtils.GetStringResource("Language_Error_Saving_Playlists"),
                                        ResourceUtils.GetStringResource("Language_Ok"),
                                        true,
                                        ResourceUtils.GetStringResource("Language_Log_File"));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Exception: {0}", ex.Message);

                            this.dialogService.ShowNotification(
                                0xe711,
                                16,
                                ResourceUtils.GetStringResource("Language_Error"),
                                ResourceUtils.GetStringResource("Language_Error_Saving_Playlists"),
                                ResourceUtils.GetStringResource("Language_Ok"),
                                true,
                                ResourceUtils.GetStringResource("Language_Log_File"));
                        }
                    }

                }
                else if (this.SelectedPlaylists.Count == 1)
                {
                    // Save 1 playlist
                    // ---------------
                    var dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = FileUtils.SanitizeFilename(this.SelectedPlaylists[0].PlaylistName);
                    dlg.DefaultExt = FileFormats.M3U;
                    dlg.Filter = string.Concat(ResourceUtils.GetStringResource("Language_Playlists"), " (", FileFormats.M3U, ")|*", FileFormats.M3U);

                    if ((bool)dlg.ShowDialog())
                    {
                        try
                        {
                            ExportPlaylistsResult result = await this.collectionService.ExportPlaylistAsync(this.SelectedPlaylists[0], dlg.FileName, false);

                            if (result == ExportPlaylistsResult.Error)
                            {
                                this.dialogService.ShowNotification(
                                        0xe711,
                                        16,
                                        ResourceUtils.GetStringResource("Language_Error"),
                                        ResourceUtils.GetStringResource("Language_Error_Saving_Playlist"),
                                        ResourceUtils.GetStringResource("Language_Ok"),
                                        true,
                                        ResourceUtils.GetStringResource("Language_Log_File"));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Exception: {0}", ex.Message);

                            this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Saving_Playlist"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                        }
                    }
                }
                else
                {
                    // Should not happen
                }
            }
        }
        #endregion

        #region Overrides
        protected async override Task FillListsAsync()
        {
            await this.GetPlaylistsAsync();
        }


        protected override void Unsubscribe()
        {
        }


        protected override void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();
        }

        protected override void RefreshLanguage()
        {
            // Do Nothing
        }
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            // We don't allow drag and drop when more as 1 playlist is selected
            if (this.selectedPlaylists != null && this.selectedPlaylists.Count == 1)
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);

                try
                {
                    dropInfo.NotHandled = true;
                }
                catch (Exception ex)
                {
                    dropInfo.NotHandled = false;
                    LogClient.Error("Could not drag tracks. Exception: {0}", ex.Message);
                }
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            try
            {
                var tracks = new List<MergedTrack>();

                foreach (var item in dropInfo.TargetCollection)
                {
                    tracks.Add(((MergedTrackViewModel)item).Track);
                }

                await this.playlistRepository.DeleteTracksFromPlaylistAsync(tracks, selectedPlaylists[0]);
                await this.playlistRepository.AddTracksToPlaylistAsync(tracks, selectedPlaylists[0].PlaylistName);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not drop tracks. Exception: {0}", ex.Message);
            }
        }
        #endregion
    }
}
