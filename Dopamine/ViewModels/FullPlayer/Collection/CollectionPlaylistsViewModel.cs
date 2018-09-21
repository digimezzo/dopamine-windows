using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.File;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.ViewModels.Common.Base;
using Dopamine.Views.FullPlayer.Collection;
using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionPlaylistsViewModel : TracksViewModelBaseWithTrackArt, IDropTarget
    {
        private ObservableCollection<PlaylistViewModel> playlists;
        private PlaylistViewModel selectedPlaylist;
        private IDialogService dialogService;
        private IPlaylistService playlistService;
        private IPlaybackService playbackService;
        private IFileService fileService;
        private IEventAggregator eventAggregator;
        private IContainerProvider container;
        private double leftPaneWidthPercent;

        public DelegateCommand AddPlaylistToNowPlayingCommand { get; set; }

        public DelegateCommand ShuffleSelectedPlaylistCommand { get; set; }

        public DelegateCommand NewPlaylistCommand { get; set; }

        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }

        public DelegateCommand ImportPlaylistsCommand { get; set; }

        public DelegateCommand DeleteSelectedPlaylistCommand { get; set; }

        public DelegateCommand<PlaylistViewModel> DeletePlaylistCommand { get; set; }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public CollectionPlaylistsViewModel(IContainerProvider container, IDialogService dialogService,
            IPlaybackService playbackService, IPlaylistService playlistService,
            IFileService fileService, IEventAggregator eventAggregator) : base(container)
        {
            this.dialogService = dialogService;
            this.playlistService = playlistService;
            this.playbackService = playbackService;
            this.fileService = fileService;
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.container = container;

            // Events
            this.playlistService.PlaylistFolderChanged += PlaylistService_PlaylistFolderChanged;
            this.playlistService.TracksAdded += PlaylistService_TracksAdded;
            this.playlistService.TracksDeleted += PlaylistService_TracksDeleted;

            // Commands
            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.DeletePlaylistCommand = new DelegateCommand<PlaylistViewModel>(async (playlist) => await this.ConfirmDeletePlaylistAsync(playlist));
            this.ImportPlaylistsCommand = new DelegateCommand(async () => await this.ImportPlaylistsAsync());
            this.AddPlaylistToNowPlayingCommand = new DelegateCommand(async () => await this.AddPlaylistToNowPlayingAsync());
            this.ShuffleSelectedPlaylistCommand = new DelegateCommand(async () => await this.ShuffleSelectedPlaylistAsync());
            this.LoadedCommand = new DelegateCommand(async () => await this.LoadedCommandAsync());
            this.NewPlaylistCommand = new DelegateCommand(async () => await this.ConfirmCreateNewPlaylistAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());

            this.DeleteSelectedPlaylistCommand = new DelegateCommand(async () =>
            {
                if (this.IsPlaylistSelected)
                {
                    await this.ConfirmDeletePlaylistAsync(this.SelectedPlaylist);
                }
            });

            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.SettingValue;
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.SettingValue;
                }
            };

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }

        private async void PlaylistService_PlaylistFolderChanged(object sender, EventArgs e)
        {
            await this.FillListsAsync();
        }

        public string PlaylistsTarget => "ListBoxPlaylists";

        public string TracksTarget => "ListBoxTracks";

        public bool IsPlaylistSelected => this.selectedPlaylist != null;

        public long PlaylistsCount => this.playlists == null ? 0 : this.playlists.Count;

        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return this.playlists; }
            set { SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.playlists, value); }
        }

        public PlaylistViewModel SelectedPlaylist
        {
            get { return this.selectedPlaylist; }
            set
            {
                SetProperty<PlaylistViewModel>(ref this.selectedPlaylist, value);

                if (value != null)
                {
                    this.GetTracksAsync();
                }
                else
                {
                    this.ClearTracks();
                }
            }
        }

        public string SelectedPlaylistName
        {
            get
            {
                if (this.SelectedPlaylist != null && !string.IsNullOrEmpty(this.SelectedPlaylist.Name))
                {
                    return this.SelectedPlaylist.Name;
                }

                return null;
            }
        }

        private async Task ShuffleSelectedPlaylistAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracksAsync(this.SelectedPlaylist);
            await this.playbackService.EnqueueAsync(tracks, true, false);
        }

        private async Task AddPlaylistToNowPlayingAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracksAsync(this.SelectedPlaylist);
            EnqueueResult result = await this.playbackService.AddToQueueAsync(tracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
        }

        protected async Task GetTracksAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracksAsync(this.SelectedPlaylist);
            await this.GetTracksCommonAsync(tracks, TrackOrder.None);
        }

        protected async Task ImportPlaylistsAsync(IList<string> playlistPaths = null)
        {
            if (playlistPaths == null)
            {
                // Set up the file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = Application.Current.FindResource("Language_Import_Playlists").ToString();
                dlg.DefaultExt = FileFormats.M3U; // Default file extension
                dlg.Multiselect = true;

                // Filter files by extension
                dlg.Filter = $"{ResourceUtils.GetString("Language_Playlists")} {this.playlistService.DialogFileFilter}";

                // Show the file dialog box
                bool? dialogResult = dlg.ShowDialog();

                // Process the file dialog box result
                if (!(bool)dialogResult)
                {
                    return;
                }

                playlistPaths = dlg.FileNames;
            }

            ImportPlaylistResult result = await this.playlistService.ImportPlaylistsAsync(playlistPaths);

            if (result == ImportPlaylistResult.Error)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetString("Language_Error"),
                    ResourceUtils.GetString("Language_Error_Importing_Playlists"),
                    ResourceUtils.GetString("Language_Ok"),
                    true,
                    ResourceUtils.GetString("Language_Log_File"));
            }
        }

        private async Task ClearTracks()
        {
            await this.GetTracksCommonAsync(new List<TrackViewModel>(), TrackOrder.None);
        }

        protected void TrySelectFirstPlaylist()
        {
            try
            {
                if (this.Playlists.Count > 0)
                {
                    this.SelectedPlaylist = this.Playlists[0];
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while selecting the playlist. Exception: {0}", ex.Message);
            }
        }

        protected override async Task FillListsAsync()
        {
            await this.GetPlaylistsAsync();
            await this.GetTracksAsync();
        }

        protected override async Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad())
            {
                return;
            }

            await Task.Delay(Constants.CommonListLoadDelay); // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }

        private async Task ConfirmDeletePlaylistAsync(PlaylistViewModel playlist)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetString("Language_Delete"),
                ResourceUtils.GetString("Language_Are_You_Sure_To_Delete_Playlist").Replace("{playlistname}", playlist.Name),
                ResourceUtils.GetString("Language_Yes"),
                ResourceUtils.GetString("Language_No")))
            {
                await this.DeletePlaylistAsync(playlist);
            }
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (!this.IsPlaylistSelected)
            {
                return;
            }

            PlaylistViewModel oldPlaylist = this.SelectedPlaylist;
            string newPlaylistName = oldPlaylist.Name;

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_Rename_Playlist"),
                ResourceUtils.GetString("Language_Enter_New_Name_For_Playlist").Replace("{playlistname}", oldPlaylist.Name),
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ref newPlaylistName))
            {
                RenamePlaylistResult result = await this.playlistService.RenamePlaylistAsync(this.SelectedPlaylist, newPlaylistName);

                switch (result)
                {
                    case RenamePlaylistResult.Duplicate:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Already_Exists"),
                            ResourceUtils.GetString("Language_Already_Playlist_With_That_Name").Replace("{playlistname}", newPlaylistName),
                            ResourceUtils.GetString("Language_Ok"),
                            false,
                            string.Empty);
                        break;
                    case RenamePlaylistResult.Error:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Error"),
                            ResourceUtils.GetString("Language_Error_Renaming_Playlist"),
                            ResourceUtils.GetString("Language_Ok"),
                            true,
                            ResourceUtils.GetString("Language_Log_File"));
                        break;
                    case RenamePlaylistResult.Blank:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetString("Language_Error"),
                            ResourceUtils.GetString("Language_Provide_Playlist_Name"),
                            ResourceUtils.GetString("Language_Ok"),
                            false,
                            string.Empty);
                        break;
                    default:
                        // Never happens
                        break;
                }
            }
        }

        private async void PlaylistService_TracksDeleted(PlaylistViewModel playlist)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && playlist.Equals(this.SelectedPlaylist))
            {
                await this.GetTracksAsync();
            }
        }

        private async void PlaylistService_TracksAdded(int numberTracksAdded, string playlistName)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && string.Equals(this.SelectedPlaylistName, playlistName, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.GetTracksAsync();
            }
        }

        private async Task ConfirmCreateNewPlaylistAsync()
        {
            string proposedPlaylistName = await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist"));

            //if (this.dialogService.ShowInputDialog(
            //    0xea37,
            //    16,
            //    ResourceUtils.GetString("Language_New_Playlist"),
            //    ResourceUtils.GetString("Language_Enter_Name_For_New_Playlist"),
            //    ResourceUtils.GetString("Language_Ok"),
            //    ResourceUtils.GetString("Language_Cancel"),
            //    ref proposedPlaylistName))
            //{
            //    await this.CreateNewPlaylistAsync(proposedPlaylistName);
            //}

            CollectionPlaylistsCreator view = this.container.Resolve<CollectionPlaylistsCreator>();
            view.DataContext = this.container.Resolve<CollectionPlaylistsCreatorViewModel>();

            this.dialogService.ShowCustomDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_New_Playlist"),
                view,
                500,
                0,
                false,
                true,
                false,
                true,
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                null);
        }

        private async Task CreateNewPlaylistAsync(string playlistName)
        {
            CreateNewPlaylistResult result = await this.playlistService.CreateNewPlaylistAsync(playlistName, PlaylistType.Static);

            switch (result)
            {
                case CreateNewPlaylistResult.Duplicate:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Already_Exists"),
                        ResourceUtils.GetString("Language_Already_Playlist_With_That_Name").Replace("{playlistname}", playlistName),
                        ResourceUtils.GetString("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case CreateNewPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case CreateNewPlaylistResult.Blank:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetString("Language_Ok"),
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
            try
            {
                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>(await this.playlistService.GetAllPlaylistsAsync());

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));

            // Select the first playlist
            this.TrySelectFirstPlaylist();
        }

        private async Task DeletePlaylistAsync(PlaylistViewModel playlist)
        {
            DeletePlaylistsResult result = await this.playlistService.DeletePlaylistAsync(playlist);

            if (result == DeletePlaylistsResult.Error)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetString("Language_Error"),
                    ResourceUtils.GetString("Language_Error_Deleting_Playlist").Replace("{playlistname}", playlist.Name),
                    ResourceUtils.GetString("Language_Ok"),
                    true,
                    ResourceUtils.GetString("Language_Log_File"));
            }
        }

        private async Task DeleteTracksFromPlaylistsAsync()
        {
            if (!this.IsPlaylistSelected)
            {
                return;
            }

            string question = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Songs_From_Playlist").Replace("{playlistname}", this.SelectedPlaylistName);

            if (this.SelectedTracks.Count == 1)
            {
                question = ResourceUtils.GetString("Language_Are_You_Sure_To_Remove_Song_From_Playlist").Replace("{playlistname}", this.SelectedPlaylistName);
            }

            if (this.dialogService.ShowConfirmation(
            0xe11b,
            16,
            ResourceUtils.GetString("Language_Delete"),
            question,
            ResourceUtils.GetString("Language_Yes"),
            ResourceUtils.GetString("Language_No")))
            {
                IList<int> selectedIndexes = await this.GetSelectedIndexesAsync();
                DeleteTracksFromPlaylistResult result = await this.playlistService.DeleteTracksFromStaticPlaylistAsync(selectedIndexes, this.SelectedPlaylist);

                if (result == DeleteTracksFromPlaylistResult.Error)
                {
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Removing_From_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                }
            }
        }

        private async Task<IList<int>> GetSelectedIndexesAsync()
        {
            IList<int> indexes = new List<int>();

            try
            {
                await Task.Run(() =>
                {
                    foreach (TrackViewModel selectedTrack in this.SelectedTracks)
                    {
                        indexes.Add(this.Tracks.IndexOf(selectedTrack));
                    }
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get the selected indexes. Exception: {0}", ex.Message);
            }

            return indexes;
        }

        private async Task ReorderSelectedPlaylistTracksAsync(IDropInfo dropInfo)
        {
            var tracks = new List<TrackViewModel>();

            await Task.Run(() =>
            {
                try
                {
                    foreach (var item in dropInfo.TargetCollection)
                    {
                        tracks.Add((TrackViewModel)item);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get the dropped tracks. Exception: {0}", ex.Message);
                }
            });

            await this.playlistService.SetStaticPlaylistOrderAsync(this.SelectedPlaylist, tracks);
        }

        private async Task AddDroppedTracksToHoveredPlaylist(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is TrackViewModel | dropInfo.Data is IList<TrackViewModel>)
                && dropInfo.TargetItem is PlaylistViewModel)
            {
                try
                {
                    PlaylistViewModel hoveredPlaylist = (PlaylistViewModel)dropInfo.TargetItem;

                    if (hoveredPlaylist.Type.Equals(PlaylistType.Smart))
                    {
                        return; // Don't add tracks to a smart playlist
                    }

                    if (hoveredPlaylist.Equals(this.SelectedPlaylist))
                    {
                        return; // Don't add tracks to the same playlist
                    }

                    var tracks = new List<TrackViewModel>();

                    await Task.Run(() =>
                    {
                        if (dropInfo.Data is TrackViewModel)
                        {
                            // We dropped a single track
                            tracks.Add((TrackViewModel)dropInfo.Data);
                        }
                        else if (dropInfo.Data is IList<TrackViewModel>)
                        {
                            // We dropped multiple tracks
                            foreach (TrackViewModel track in (IList<TrackViewModel>)dropInfo.Data)
                            {
                                tracks.Add(track);
                            }
                        }
                    });

                    await this.playlistService.AddTracksToStaticPlaylistAsync(tracks, hoveredPlaylist.Name);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not add dropped tracks to hovered playlist. Exception: {0}", ex.Message);
                }
            }
        }

        private async Task AddDroppedFilesToSelectedPlaylist(IDropInfo dropInfo)
        {
            try
            {
                IList<string> filenames = dropInfo.GetDroppedFilenames();
                IList<TrackViewModel> tracks = await this.fileService.ProcessFilesAsync(filenames);
                await this.playlistService.AddTracksToStaticPlaylistAsync(tracks, this.SelectedPlaylistName);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not add dropped files to selected playlist. Exception: {0}", ex.Message);
            }
        }

        private async Task AddDroppedFilesToHoveredPlaylist(IDropInfo dropInfo)
        {
            PlaylistViewModel hoveredPlaylist = null;
            IList<TrackViewModel> tracks = null;

            try
            {
                hoveredPlaylist = (PlaylistViewModel)dropInfo.TargetItem;
                IList<string> filenames = dropInfo.GetDroppedFilenames();
                tracks = await this.fileService.ProcessFilesAsync(filenames);

                if (hoveredPlaylist != null && tracks != null)
                {
                    await this.playlistService.AddTracksToStaticPlaylistAsync(tracks, hoveredPlaylist.Name);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not add dropped files to hovered playlist. Exception: {0}", ex.Message);
            }
        }

        private async Task AddDroppedFilesToPlaylists(IDropInfo dropInfo)
        {
            // 3 possibilities
            if (dropInfo.TargetItem is PlaylistViewModel)
            {
                // 1. Drop audio and playlist files on a playlist name: add all files 
                // (including those in the dropped playlist files) to that playlist.
                try
                {
                    PlaylistViewModel hoveredPlaylist = (PlaylistViewModel)dropInfo.TargetItem;

                    // Don't add anything to smart playlists
                    if (hoveredPlaylist.Type.Equals(PlaylistType.Smart))
                    {
                        return;
                    }

                    IList<string> filenames = dropInfo.GetDroppedFilenames();
                    IList<TrackViewModel> tracks = await this.fileService.ProcessFilesAsync(filenames);

                    if (hoveredPlaylist != null && tracks != null)
                    {
                        await this.playlistService.AddTracksToStaticPlaylistAsync(tracks, hoveredPlaylist.Name);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not add dropped files to hovered playlist. Exception: {0}", ex.Message);
                }
            }
            else if (dropInfo.TargetItem == null)
            {
                string uniquePlaylistName = await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist"));
                IList<string> allFilenames = dropInfo.GetDroppedFilenames();
                IList<string> audioFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedAudioFile(f)).ToList();
                IList<string> playlistFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedStaticPlaylistFile(f)).ToList();

                // 2. Drop audio files in empty part of list: add these files to a new unique playlist
                IList<TrackViewModel> audiofileTracks = await this.fileService.ProcessFilesAsync(audioFileNames);

                if (audiofileTracks != null && audiofileTracks.Count > 0)
                {
                    await this.playlistService.CreateNewPlaylistAsync(uniquePlaylistName, PlaylistType.Static);
                    await this.playlistService.AddTracksToStaticPlaylistAsync(audiofileTracks, uniquePlaylistName);
                }

                // 3. Drop playlist files in empty part of list: add the playlist with a unique name
                await this.ImportPlaylistsAsync(playlistFileNames);
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                // We don't allow dragging playlists
                if (dropInfo.Data is PlaylistViewModel)
                {
                    return;
                }

                // If we're dragging files, we need to be dragging valid files.
                if (dropInfo.IsDraggingFiles() &&
                    !(dropInfo.IsDraggingMediaFiles() || dropInfo.IsDraggingStaticPlaylistFiles() || dropInfo.IsDraggingSmartPlaylistFiles()))
                {
                    return;
                }

                // In all other cases, allow dragging.
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
                dropInfo.NotHandled = true;
            }
            catch (Exception ex)
            {
                dropInfo.NotHandled = false;
                LogClient.Error("Could not drag tracks. Exception: {0}", ex.Message);
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            try
            {
                ListBox target = dropInfo.VisualTarget as ListBox;

                if (target.Name.Equals(this.PlaylistsTarget))
                {
                    // Dragging to the Playlists listbox
                    if (dropInfo.IsDraggingFiles())
                    {
                        await this.AddDroppedFilesToPlaylists(dropInfo);
                    }
                    else
                    {
                        await this.AddDroppedTracksToHoveredPlaylist(dropInfo);
                    }
                }
                else if (target.Name.Equals(this.TracksTarget))
                {
                    // Dragging to the Tracks listbox
                    if (dropInfo.IsDraggingFiles())
                    {
                        await this.AddDroppedFilesToSelectedPlaylist(dropInfo);
                    }
                    else
                    {
                        GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo); // Automatically performs built-in reorder
                        await this.ReorderSelectedPlaylistTracksAsync(dropInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not perform drop. Exception: {0}", ex.Message);
            }
        }
    }
}
