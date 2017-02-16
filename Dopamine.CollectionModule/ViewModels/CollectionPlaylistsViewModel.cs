using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Helpers;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Playlist;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionPlaylistsViewModel : PlaylistViewModelBase, IDropTarget
    {
        #region Variables
        // Lists
        private ObservableCollection<PlaylistViewModel> playlists;
        private string selectedPlaylist;

        // Flags
        private bool isLoadingPlaylists;

        // Other
        private long playlistsCount;
        private double leftPaneWidthPercent;
        #endregion

        #region Commands
        public DelegateCommand NewPlaylistCommand { get; set; }
        public DelegateCommand OpenPlaylistCommand { get; set; }
        public DelegateCommand<string> DeletePlaylistByNameCommand { get; set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }
        public DelegateCommand DeleteSelectedPlaylistCommand { get; set; }
        public DelegateCommand<object> SelectedPlaylistCommand { get; set; }
        public DelegateCommand AddPlaylistToNowPlayingCommand { get; set; }
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

        public bool IsPlaylistSelected
        {
            get { return !string.IsNullOrEmpty(this.selectedPlaylist); }
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

        public string SelectedPlaylist
        {
            get { return this.selectedPlaylist; }
            set { SetProperty<string>(ref this.selectedPlaylist, value); }
        }

        public long PlaylistsCount
        {
            get { return this.playlistsCount; }
            set { SetProperty<long>(ref this.playlistsCount, value); }
        }
        #endregion

        #region Construction
        public CollectionPlaylistsViewModel(IUnityContainer container, IEventAggregator eventAggregator, IDialogService dialogService, IPlaylistService playlistService)
           : base(container)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.playlistService = playlistService;

            // Commands
            this.LoadedCommand = new DelegateCommand(async () => await this.LoadedCommandAsync());
            this.NewPlaylistCommand = new DelegateCommand(async () => await this.ConfirmAddPlaylistAsync());
            this.OpenPlaylistCommand = new DelegateCommand(async () => await this.OpenPlaylistAsync());
            this.DeletePlaylistByNameCommand = new DelegateCommand<string>(async (playlistName) => await this.ConfirmDeletePlaylistAsync(playlistName));
            this.DeleteSelectedPlaylistCommand = new DelegateCommand(async () =>
            {
                if (this.IsPlaylistSelected) await this.ConfirmDeletePlaylistAsync(this.SelectedPlaylist);
            });

            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());
            this.SelectedPlaylistCommand = new DelegateCommand<object>(async (parameter) => await SelectedPlaylistHandlerAsync(parameter));
            this.AddPlaylistToNowPlayingCommand = new DelegateCommand(async () => await this.AddPlaylistToNowPlayingAsync());

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(enableRating => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe(enableLove => this.EnableLove = enableLove);

            // PlaylistService
            this.playlistService.TracksAdded += async (_, __) => await this.FillListsAsync();
            this.playlistService.TracksDeleted += async (deletedPaths, playlist) => await this.UpdateDeletedTracksAsync(deletedPaths, playlist);
            this.playlistService.PlaylistAdded += (addedPlaylist) => this.UpdateAddedPlaylist(addedPlaylist);
            this.playlistService.PlaylistDeleted += (deletedPlaylist) => this.UpdateDeletedPlaylist(deletedPlaylist);
            this.playlistService.PlaylistRenamed += (oldPlaylist, newPlaylist) => this.UpdateRenamedPlaylist(oldPlaylist, newPlaylist);

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }
        #endregion

        #region Private
        private void UpdateAddedPlaylist(string addedPlaylist)
        {
            this.Playlists.Add(new PlaylistViewModel() { Playlist = addedPlaylist });
        }

        private void UpdateDeletedPlaylist(string deletedPlaylist)
        {
            this.Playlists.Remove(new PlaylistViewModel() { Playlist = deletedPlaylist });
        }

        private void UpdateRenamedPlaylist(string oldPlaylist, string newPlaylist)
        {
            // Remove the old playlist
            var oldVm = new PlaylistViewModel() { Playlist = oldPlaylist };
            if (this.Playlists.Contains(oldVm)) this.Playlists.Remove(oldVm);

            // Add the new playlist
            this.Playlists.Add(new PlaylistViewModel() { Playlist = newPlaylist });
        }

        private async Task UpdateDeletedTracksAsync(List<string> deletedPaths, string playlist)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && string.Equals(this.SelectedPlaylist, playlist, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.GetTracksAsync();
            }
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

        private async Task AddPlaylistAsync(string playlistName)
        {
            AddPlaylistResult result = await this.playlistService.AddPlaylistAsync(playlistName);

            switch (result)
            {
                case AddPlaylistResult.Duplicate:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Already_Exists"),
                        ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", playlistName),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case AddPlaylistResult.Error:
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
                    break;
            }
        }

        private async Task GetPlaylistsAsync()
        {
            // Notify the user
            this.IsLoadingPlaylists = true;

            try
            {
                // Get the Albums from the database
                IList<string> playlists = await this.playlistService.GetPlaylistsAsync();

                // Set the count
                this.PlaylistsCount = playlists.Count;

                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>();

                await Task.Run(() =>
                {
                    foreach (string playlist in playlists)
                    {
                        playlistViewModels.Add(new PlaylistViewModel { Playlist = playlist });
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

            // Stop notifying
            this.IsLoadingPlaylists = false;
        }

        private async Task DeletePlaylistAsync(string playlist)
        {
            DeletePlaylistsResult result = await this.playlistService.DeletePlaylistAsync(playlist);

            if (result == DeletePlaylistsResult.Error)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetStringResource("Language_Error"),
                    ResourceUtils.GetStringResource("Language_Error_Deleting_Playlist").Replace("%playlistname%", playlist),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task ConfirmDeletePlaylistAsync(string playlist)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetStringResource("Language_Delete"),
                ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", playlist),
                ResourceUtils.GetStringResource("Language_Yes"),
                ResourceUtils.GetStringResource("Language_No")))
            {
                await this.DeletePlaylistAsync(playlist);
            }
        }

        private async Task SelectedPlaylistHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedPlaylist = ((PlaylistViewModel)parameter).Playlist;
            }

            await this.GetTracksAsync();
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (!this.IsPlaylistSelected) return;

            string oldPlaylist = this.SelectedPlaylist;
            string newPlaylist = oldPlaylist;

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetStringResource("Language_Rename_Playlist"),
                ResourceUtils.GetStringResource("Language_Enter_New_Name_For_Playlist").Replace("%playlistname%", oldPlaylist),
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ref newPlaylist))
            {
                RenamePlaylistResult result = await this.playlistService.RenamePlaylistAsync(oldPlaylist, newPlaylist);

                switch (result)
                {
                    case RenamePlaylistResult.Duplicate:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetStringResource("Language_Already_Exists"),
                            ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", newPlaylist),
                            ResourceUtils.GetStringResource("Language_Ok"),
                            false,
                            string.Empty);
                        break;
                    case RenamePlaylistResult.Error:
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
                        break;
                }
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

                OpenPlaylistResult openResult = await this.playlistService.OpenPlaylistAsync(dlg.FileName);

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

        public async Task GetTracksAsync()
        {
            List<PlayableTrack> tracks = await this.playlistService.GetTracks(this.SelectedPlaylist);
            var orderedTracks = new OrderedDictionary<string, PlayableTrack>();

            await Task.Run(() =>
            {
                foreach (PlayableTrack track in tracks)
                {
                    orderedTracks.Add(Guid.NewGuid().ToString(), track);
                }
            });

            await this.GetTracksCommonAsync(orderedTracks);
        }

        private async Task DeleteTracksFromPlaylistsAsync()
        {
            if (!this.IsPlaylistSelected) return;

            string question = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Songs_From_Playlist").Replace("%playlistname%", this.SelectedPlaylist);
            if (this.SelectedTracks.Count == 1) question = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Song_From_Playlist").Replace("%playlistname%", this.SelectedPlaylist);

            if (this.dialogService.ShowConfirmation(
            0xe11b,
            16,
            ResourceUtils.GetStringResource("Language_Delete"),
            question,
            ResourceUtils.GetStringResource("Language_Yes"),
            ResourceUtils.GetStringResource("Language_No")))
            {
                DeleteTracksFromPlaylistResult result = await this.playlistService.DeleteTracksFromPlaylistAsync(this.SelectedTracks.Select(t => t.Value).ToList(), this.SelectedPlaylist);

                if (result == DeleteTracksFromPlaylistResult.Error)
                {
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Removing_From_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                }
            }
        }

        protected async Task AddPlaylistToNowPlayingAsync()
        {
            List<PlayableTrack> tracks = await this.playlistService.GetTracks(this.SelectedPlaylist);

            EnqueueResult result = await this.playbackService.AddToQueueAsync(tracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as IDataObject;

            bool isDraggingFiles = dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop);
            bool isDraggingValidFiles = false;

            if (isDraggingFiles)
            {
                var basicDataObject = dropInfo.Data as DataObject;

                try
                {
                    var filenames = basicDataObject.GetFileDropList();
                    var supportedExtensions = FileFormats.SupportedMediaExtensions.Concat(FileFormats.SupportedPlaylistExtensions).ToArray();

                    foreach (string filename in filenames)
                    {
                        if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                        {
                            isDraggingValidFiles = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not detect if we're dragging valid files. Exception: {0}", ex.Message);
                }
            }

            // Dragging is only possible when 1 playlist is selected, otherwise we 
            // don't know in which playlist the tracks or fiels should be dropped.
            // When dragging files, allow only valid files.
            if (this.IsPlaylistSelected & (!isDraggingFiles | isDraggingValidFiles))
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
                var tracks = new List<PlayableTrack>();

                foreach (var item in dropInfo.TargetCollection)
                {
                    tracks.Add(((KeyValuePair<string, TrackViewModel>)item).Value.Track);
                }

                await this.playlistService.SetPlaylistOrderAsync(tracks, this.SelectedPlaylist);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not drop tracks. Exception: {0}", ex.Message);
            }
        }
        #endregion

        #region Overrides
        protected override async Task FillListsAsync()
        {
            await this.GetPlaylistsAsync();
            await this.GetTracksAsync();
        }

        protected override async Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.CommonListLoadDelay); // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }

        protected override void Subscribe()
        {
            // Not required here
        }

        protected override void Unsubscribe()
        {
            // Not required here
        }
        #endregion
    }
}
