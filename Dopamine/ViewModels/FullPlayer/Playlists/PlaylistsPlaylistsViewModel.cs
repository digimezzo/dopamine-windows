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

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsPlaylistsViewModel : PlaylistsViewModelBase, IDropTarget
    {
        private IFileService fileService;
        private IPlaylistService playlistService;
        private IDialogService dialogService;
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private double leftPaneWidthPercent;

        public DelegateCommand<string> DeletePlaylistByNameCommand { get; set; }

        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }

        public DelegateCommand DeleteSelectedPlaylistCommand { get; set; }

        public DelegateCommand AddPlaylistToNowPlayingCommand { get; set; }

        public DelegateCommand ShuffleSelectedPlaylistCommand { get; set; }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public PlaylistsPlaylistsViewModel(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.fileService = container.Resolve<IFileService>();
            this.playlistService = container.Resolve<IPlaylistService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();
            this.dialogService = container.Resolve<IDialogService>();

            // Commands
            this.LoadedCommand = new DelegateCommand(async () => await this.LoadedCommandAsync());
            this.NewPlaylistCommand = new DelegateCommand(async () => await this.ConfirmAddPlaylistAsync());
            this.ImportPlaylistsCommand = new DelegateCommand(async () => await this.ImportPlaylistsAsync());
            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());
            this.AddPlaylistToNowPlayingCommand = new DelegateCommand(async () => await this.AddPlaylistToNowPlayingAsync());
            this.DeletePlaylistByNameCommand = new DelegateCommand<string>(async (playlistName) => await this.ConfirmDeletePlaylistAsync(playlistName));
            this.ShuffleSelectedPlaylistCommand = new DelegateCommand(async () => await this.ShuffleSelectedPlaylistAsync());

            this.DeleteSelectedPlaylistCommand = new DelegateCommand(async () =>
            {
                if (this.IsPlaylistSelected)
                {
                    await this.ConfirmDeletePlaylistAsync(this.SelectedPlaylistName);
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

            // Events
            this.playlistService.TracksAdded += PlaylistService_TracksAdded;
            this.playlistService.TracksDeleted += PlaylistService_TracksDeleted;
            this.playlistService.PlaylistAdded += PlaylistAddedHandler;
            this.playlistService.PlaylistDeleted += PlaylistService_PlaylistDeleted;
            this.playlistService.PlaylistRenamed += PlaylistService_PlaylistRenamed;
            this.playlistService.PlaylistFolderChanged += PlaylistService_PlaylistFolderChanged;

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }

        private async void PlaylistService_PlaylistFolderChanged(object sender, EventArgs e)
        {
            await this.FillListsAsync();
        }

        private async void PlaylistService_TracksDeleted(string playlistName)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && string.Equals(this.SelectedPlaylistName, playlistName, StringComparison.InvariantCultureIgnoreCase))
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

        private void PlaylistService_PlaylistRenamed(PlaylistViewModel oldPlaylist, PlaylistViewModel newPlaylist)
        {
            // Remove the old playlist
            if (this.Playlists.Contains(oldPlaylist))
            {
                this.Playlists.Remove(oldPlaylist);
            }

            // Add the new playlist
            this.Playlists.Add(newPlaylist);
        }

        private void PlaylistService_PlaylistDeleted(PlaylistViewModel deletedPlaylist)
        {
            this.Playlists.Remove(deletedPlaylist);

            // If the selected playlist was deleted, select the first playlist.
            if (this.SelectedPlaylist == null)
            {
                this.TrySelectFirstPlaylist();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));
        }

        private async Task ConfirmAddPlaylistAsync()
        {
            string responseText = await this.playlistService.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist"));

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_New_Playlist"),
                ResourceUtils.GetString("Language_Enter_Name_For_New_Playlist"),
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
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
                        ResourceUtils.GetString("Language_Already_Exists"),
                        ResourceUtils.GetString("Language_Already_Playlist_With_That_Name").Replace("{playlistname}", playlistName),
                        ResourceUtils.GetString("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case AddPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case AddPlaylistResult.Blank:
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

        protected override async Task GetPlaylistsAsync()
        {
            try
            {
                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>(await this.playlistService.GetPlaylistsAsync());

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

            // Select the firts playlist
            this.TrySelectFirstPlaylist();
        }

        private async Task DeletePlaylistAsync(string playlistName)
        {
            DeletePlaylistsResult result = await this.playlistService.DeletePlaylistAsync(playlistName);

            if (result == DeletePlaylistsResult.Error)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetString("Language_Error"),
                    ResourceUtils.GetString("Language_Error_Deleting_Playlist").Replace("{playlistname}", playlistName),
                    ResourceUtils.GetString("Language_Ok"),
                    true,
                    ResourceUtils.GetString("Language_Log_File"));
            }
        }

        private async Task ConfirmDeletePlaylistAsync(string playlistName)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetString("Language_Delete"),
                ResourceUtils.GetString("Language_Are_You_Sure_To_Delete_Playlist").Replace("{playlistname}", playlistName),
                ResourceUtils.GetString("Language_Yes"),
                ResourceUtils.GetString("Language_No")))
            {
                await this.DeletePlaylistAsync(playlistName);
            }
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (!this.IsPlaylistSelected)
            {
                return;
            }

            string oldPlaylistName = this.SelectedPlaylistName;
            string newPlaylistName = oldPlaylistName;

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_Rename_Playlist"),
                ResourceUtils.GetString("Language_Enter_New_Name_For_Playlist").Replace("{playlistname}", oldPlaylistName),
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ref newPlaylistName))
            {
                RenamePlaylistResult result = await this.playlistService.RenamePlaylistAsync(oldPlaylistName, newPlaylistName);

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

        private async Task ImportPlaylistsAsync(IList<string> playlistPaths = null)
        {
            if (playlistPaths == null)
            {
                // Set up the file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = Application.Current.FindResource("Language_Import_Playlists").ToString();
                dlg.DefaultExt = FileFormats.M3U; // Default file extension
                dlg.Multiselect = true;

                // Filter files by extension
                dlg.Filter = ResourceUtils.GetString("Language_Playlists") + " (*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL + ")|*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL;

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

        protected override async Task GetTracksAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);
            await this.GetTracksCommonAsync(tracks, TrackOrder.None);
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
                DeleteTracksFromPlaylistResult result = await this.playlistService.DeleteTracksFromPlaylistAsync(selectedIndexes, this.SelectedPlaylistName);

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

        private async Task ShuffleSelectedPlaylistAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);
            await this.playbackService.EnqueueAsync(tracks, true, false);
        }

        private async Task AddPlaylistToNowPlayingAsync()
        {
            IList<TrackViewModel> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);
            EnqueueResult result = await this.playbackService.AddToQueueAsync(tracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
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

            await this.playlistService.SetPlaylistOrderAsync(tracks, this.SelectedPlaylistName);
        }

        private async Task AddDroppedTracksToHoveredPlaylist(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is TrackViewModel | dropInfo.Data is IList<TrackViewModel>)
                && dropInfo.TargetItem is PlaylistViewModel)
            {
                try
                {
                    string hoveredPlaylistName = ((PlaylistViewModel)dropInfo.TargetItem).Name;

                    if (hoveredPlaylistName.Equals(this.SelectedPlaylistName))
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

                    await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylistName);
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
                await this.playlistService.AddTracksToPlaylistAsync(tracks, this.SelectedPlaylistName);
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
                    await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylist.Name);
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
                    IList<string> filenames = dropInfo.GetDroppedFilenames();
                    IList<TrackViewModel> tracks = await this.fileService.ProcessFilesAsync(filenames);

                    if (hoveredPlaylist != null && tracks != null)
                    {
                        await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylist.Name);
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
                IList<string> playlistFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedPlaylistFile(f)).ToList();

                // 2. Drop audio files in empty part of list: add these files to a new unique playlist
                IList<TrackViewModel> audiofileTracks = await this.fileService.ProcessFilesAsync(audioFileNames);

                if (audiofileTracks != null && audiofileTracks.Count > 0)
                {
                    await this.playlistService.AddPlaylistAsync(uniquePlaylistName);
                    await this.playlistService.AddTracksToPlaylistAsync(audiofileTracks, uniquePlaylistName);
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
                bool isDraggingFiles = dropInfo.IsDraggingFiles();
                bool isDraggingValidFiles = false;

                if (isDraggingFiles)
                {
                    isDraggingValidFiles = dropInfo.IsDraggingMediaFiles() || dropInfo.IsDraggingPlaylistFiles();
                }

                if (isDraggingFiles & !isDraggingValidFiles)
                {
                    return;
                }

                // If we're dragging into the list of tracks, there must be playlists, and a playlist must be selected.
                ListBox target = dropInfo.VisualTarget as ListBox;

                if (target.Name.Equals(this.TracksTarget) && (this.Playlists == null || this.Playlists.Count == 0 || this.SelectedPlaylist == null))
                {
                    return;
                }

                // If we're dragging tracks into the list of playlists, we cannot drag to the selected playlist.
                string hoveredPlaylistName = null;

                if (dropInfo.TargetItem != null && dropInfo.TargetItem is PlaylistViewModel)
                {
                    hoveredPlaylistName = ((PlaylistViewModel)dropInfo.TargetItem).Name;
                }

                if (!isDraggingFiles && target.Name.Equals(this.PlaylistsTarget) &&
                    !string.IsNullOrEmpty(hoveredPlaylistName) &&
                    !string.IsNullOrEmpty(this.SelectedPlaylistName) &&
                    hoveredPlaylistName.Equals(this.SelectedPlaylistName))
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
