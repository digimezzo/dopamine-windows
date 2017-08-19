using Dopamine.Core.Logging;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Helpers;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.File;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Playlist;
using Dopamine.Core.Database;
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
using System.Windows.Controls;
using Dopamine.Core.Base;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionPlaylistsViewModel : PlaylistViewModelBase, IDropTarget
    {
        #region Variables
        private IFileService fileService;
        private IPlaylistService playlistService;
        private IDialogService dialogService;
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private ObservableCollection<PlaylistViewModel> playlists;
        private PlaylistViewModel selectedPlaylist;
        private bool isLoadingPlaylists;
        private string playlistsTarget = "ListBoxPlaylists";
        private string tracksTarget = "ListBoxTracks";
        private long playlistsCount;
        private double leftPaneWidthPercent;
        #endregion

        #region Commands
        public DelegateCommand NewPlaylistCommand { get; set; }
        public DelegateCommand OpenPlaylistCommand { get; set; }
        public DelegateCommand<string> DeletePlaylistByNameCommand { get; set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }
        public DelegateCommand DeleteSelectedPlaylistCommand { get; set; }
        public DelegateCommand AddPlaylistToNowPlayingCommand { get; set; }
        public DelegateCommand ShuffleSelectedPlaylistCommand { get; set; }
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

        public bool IsPlaylistSelected => this.selectedPlaylist != null;

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

        public PlaylistViewModel SelectedPlaylist
        {
            get { return this.selectedPlaylist; }
            set
            {
                SetProperty<PlaylistViewModel>(ref this.selectedPlaylist, value);

                if(value != null)
                {
                    this.GetTracksAsync();
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

        public long PlaylistsCount
        {
            get { return this.playlistsCount; }
            set { SetProperty<long>(ref this.playlistsCount, value); }
        }
        #endregion

        #region Construction
        public CollectionPlaylistsViewModel(IUnityContainer container)
           : base(container)
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
            this.OpenPlaylistCommand = new DelegateCommand(async () => await this.OpenPlaylistAsync());
            this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());
            this.AddPlaylistToNowPlayingCommand = new DelegateCommand(async () => await this.AddPlaylistToNowPlayingAsync());
            this.DeletePlaylistByNameCommand = new DelegateCommand<string>(async (playlistName) => await this.ConfirmDeletePlaylistAsync(playlistName));
            this.ShuffleSelectedPlaylistCommand = new DelegateCommand(async () => await this.ShuffleSelectedPlaylistAsync());

            this.DeleteSelectedPlaylistCommand = new DelegateCommand(async () =>
            {
                if (this.IsPlaylistSelected) await this.ConfirmDeletePlaylistAsync(this.SelectedPlaylistName);
            });

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(enableRating => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe(enableLove => this.EnableLove = enableLove);
            this.playlistService.TracksAdded += async (numberTracksAdded, playlistName) => await this.UpdateAddedTracksAsync(playlistName);
            this.playlistService.TracksDeleted += async (playlistName) => await this.UpdateDeletedTracksAsync(playlistName);
            this.playlistService.PlaylistAdded += (addedPlaylistName) => this.UpdateAddedPlaylist(addedPlaylistName);
            this.playlistService.PlaylistDeleted += (deletedPlaylistName) => this.UpdateDeletedPlaylist(deletedPlaylistName);
            this.playlistService.PlaylistRenamed += (oldPlaylistName, newPlaylistName) => this.UpdateRenamedPlaylist(oldPlaylistName, newPlaylistName);
            this.playlistService.PlaylistFolderChanged += async (_, __) => await this.FillListsAsync();
            
            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }
        #endregion

        #region Private
        private void TrySelectFirstPlaylist()
        {
            try
            {
                if (this.Playlists.Count > 0) this.SelectedPlaylist = this.Playlists[0];
            }
            catch (Exception ex)
            {
                CoreLogger.Error("An error occured while selecting the playlist. Exception: {0}", ex.Message);
            }
        }

        private void UpdateAddedPlaylist(string addedPlaylistName)
        {
            this.Playlists.Add(new PlaylistViewModel() { Name = addedPlaylistName });

            // If there is only 1 playlist, automatically select it.
            if (this.Playlists != null && this.Playlists.Count == 1)
            {
                this.TrySelectFirstPlaylist();
            }
        }

        private void UpdateDeletedPlaylist(string deletedPlaylistName)
        {
            this.Playlists.Remove(new PlaylistViewModel() { Name = deletedPlaylistName });

            // If the selected playlist was deleted, select the first playlist.
            if (this.SelectedPlaylist == null)
            {
                this.TrySelectFirstPlaylist();
            }
        }

        private void UpdateRenamedPlaylist(string oldPlaylistName, string newPlaylistName)
        {
            // Remove the old playlist
            var oldVm = new PlaylistViewModel() { Name = oldPlaylistName };
            if (this.Playlists.Contains(oldVm)) this.Playlists.Remove(oldVm);

            // Add the new playlist
            this.Playlists.Add(new PlaylistViewModel() { Name = newPlaylistName });
        }

        private async Task UpdateAddedTracksAsync(string playlistName)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && string.Equals(this.SelectedPlaylistName, playlistName, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.GetTracksAsync();
            }
        }

        private async Task UpdateDeletedTracksAsync(string playlistName)
        {
            // Only update the tracks, if the selected playlist was modified.
            if (this.IsPlaylistSelected && string.Equals(this.SelectedPlaylistName, playlistName, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.GetTracksAsync();
            }
        }

        private async Task ConfirmAddPlaylistAsync()
        {
            string responseText = await this.playlistService.GetUniquePlaylistAsync(ResourceUtils.GetStringResource("Language_New_Playlist"));

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
                        playlistViewModels.Add(new PlaylistViewModel { Name = playlist });
                    }
                });

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                CoreLogger.Error("An error occured while getting Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }

            // Stop notifying
            this.IsLoadingPlaylists = false;

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
                    ResourceUtils.GetStringResource("Language_Error"),
                    ResourceUtils.GetStringResource("Language_Error_Deleting_Playlist").Replace("%playlistname%", playlistName),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task ConfirmDeletePlaylistAsync(string playlistName)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetStringResource("Language_Delete"),
                ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", playlistName),
                ResourceUtils.GetStringResource("Language_Yes"),
                ResourceUtils.GetStringResource("Language_No")))
            {
                await this.DeletePlaylistAsync(playlistName);
            }
        }

        private async Task RenameSelectedPlaylistAsync()
        {
            if (!this.IsPlaylistSelected) return;

            string oldPlaylistName = this.SelectedPlaylistName;
            string newPlaylistName = oldPlaylistName;

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetStringResource("Language_Rename_Playlist"),
                ResourceUtils.GetStringResource("Language_Enter_New_Name_For_Playlist").Replace("%playlistname%", oldPlaylistName),
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ref newPlaylistName))
            {
                RenamePlaylistResult result = await this.playlistService.RenamePlaylistAsync(oldPlaylistName, newPlaylistName);

                switch (result)
                {
                    case RenamePlaylistResult.Duplicate:
                        this.dialogService.ShowNotification(
                            0xe711,
                            16,
                            ResourceUtils.GetStringResource("Language_Already_Exists"),
                            ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", newPlaylistName),
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
            List<PlayableTrack> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);
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

            string question = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Songs_From_Playlist").Replace("%playlistname%", this.SelectedPlaylistName);
            if (this.SelectedTracks.Count == 1) question = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Remove_Song_From_Playlist").Replace("%playlistname%", this.SelectedPlaylistName);

            if (this.dialogService.ShowConfirmation(
            0xe11b,
            16,
            ResourceUtils.GetStringResource("Language_Delete"),
            question,
            ResourceUtils.GetStringResource("Language_Yes"),
            ResourceUtils.GetStringResource("Language_No")))
            {
                List<int> selectedIndexes = await this.GetSelectedIndexesAsync();
                DeleteTracksFromPlaylistResult result = await this.playlistService.DeleteTracksFromPlaylistAsync(selectedIndexes, this.SelectedPlaylistName);

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

        private async Task<List<int>> GetSelectedIndexesAsync()
        {
            var indexes = new List<int>();

            try
            {
                await Task.Run(() =>
                {
                    List<string> trackKeys = this.Tracks.Select(t => t.Key).ToList();

                    foreach (KeyValuePair<string, PlayableTrack> selectedTrack in this.SelectedTracks)
                    {
                        indexes.Add(trackKeys.IndexOf(selectedTrack.Key));
                    }
                });
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not get the selected indexes. Exception: {0}", ex.Message);
            }

            return indexes;
        }

        private async Task ShuffleSelectedPlaylistAsync()
        {
            List<PlayableTrack> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);
            await this.playbackService.EnqueueAsync(tracks, true, false);
        }

        private async Task AddPlaylistToNowPlayingAsync()
        {
            List<PlayableTrack> tracks = await this.playlistService.GetTracks(this.SelectedPlaylistName);

            EnqueueResult result = await this.playbackService.AddToQueueAsync(tracks);

            if (!result.IsSuccess)
            {
                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private bool IsDraggingFiles(IDropInfo dropInfo)
        {
            try
            {
                var dataObject = dropInfo.Data as IDataObject;
                return dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop);
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not detect if we're dragging files. Exception: {0}", ex.Message);
            }

            return false;
        }

        private bool IsDraggingValidFiles(IDropInfo dropInfo)
        {
            try
            {
                var dataObject = dropInfo.Data as DataObject;

                var filenames = dataObject.GetFileDropList();
                var supportedExtensions = FileFormats.SupportedMediaExtensions.Concat(FileFormats.SupportedPlaylistExtensions).ToArray();

                foreach (string filename in filenames)
                {
                    if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not detect if we're dragging valid files. Exception: {0}", ex.Message);
            }

            return false;
        }

        private async Task ReorderSelectedPlaylistTracksAsync(IDropInfo dropInfo)
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                try
                {
                    foreach (var item in dropInfo.TargetCollection)
                    {
                        tracks.Add(((KeyValuePair<string, TrackViewModel>)item).Value.Track);
                    }
                }
                catch (Exception ex)
                {
                    CoreLogger.Error("Could not get the dropped tracks. Exception: {0}", ex.Message);
                }
            });

            await this.playlistService.SetPlaylistOrderAsync(tracks, this.SelectedPlaylistName);
        }

        private async Task AddDroppedTracksToHoveredPlaylist(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is KeyValuePair<string, TrackViewModel> | dropInfo.Data is List<KeyValuePair<string, TrackViewModel>>)
                && dropInfo.TargetItem is PlaylistViewModel)
            {
                try
                {
                    string hoveredPlaylistName = ((PlaylistViewModel)dropInfo.TargetItem).Name;

                    if (hoveredPlaylistName.Equals(this.SelectedPlaylistName)) return; // Don't add tracks to the same playlist

                    var tracks = new List<PlayableTrack>();

                    await Task.Run(() =>
                    {
                        if (dropInfo.Data is KeyValuePair<string, TrackViewModel>)
                        {
                            // We dropped a single track
                            tracks.Add(((KeyValuePair<string, TrackViewModel>)dropInfo.Data).Value.Track);
                        }
                        else if (dropInfo.Data is List<KeyValuePair<string, TrackViewModel>>)
                        {
                            // We dropped multiple tracks
                            foreach (KeyValuePair<string, TrackViewModel> pair in (List<KeyValuePair<string, TrackViewModel>>)dropInfo.Data)
                            {
                                tracks.Add(pair.Value.Track);
                            }
                        }
                    });

                    await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylistName);
                }
                catch (Exception ex)
                {
                    CoreLogger.Error("Could not add dropped tracks to hovered playlist. Exception: {0}", ex.Message);
                }
            }
        }

        private List<string> GetDroppedFilenames(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;

            List<string> filenames = new List<string>();

            try
            {
                filenames = dataObject.GetFileDropList().Cast<string>().ToList();
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not get the dropped filenames. Exception: {0}", ex.Message);
            }

            return filenames;
        }

        private async Task AddDroppedFilesToSelectedPlaylist(IDropInfo dropInfo)
        {
            try
            {
                var filenames = this.GetDroppedFilenames(dropInfo);
                List<PlayableTrack> tracks = await this.fileService.ProcessFilesAsync(filenames);
                await this.playlistService.AddTracksToPlaylistAsync(tracks, this.SelectedPlaylistName);
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not add dropped files to selected playlist. Exception: {0}", ex.Message);
            }
        }

        private async Task AddDroppedFilesToHoveredPlaylist(IDropInfo dropInfo)
        {
            PlaylistViewModel hoveredPlaylist = null;
            List<PlayableTrack> tracks = null;

            try
            {
                hoveredPlaylist = (PlaylistViewModel)dropInfo.TargetItem;
                var filenames = this.GetDroppedFilenames(dropInfo);
                tracks = await this.fileService.ProcessFilesAsync(filenames);

                if (hoveredPlaylist != null && tracks != null) await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylist.Name);
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not add dropped files to hovered playlist. Exception: {0}", ex.Message);
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
                    var filenames = this.GetDroppedFilenames(dropInfo);
                    List<PlayableTrack> tracks = await this.fileService.ProcessFilesAsync(filenames);

                    if (hoveredPlaylist != null && tracks != null) await this.playlistService.AddTracksToPlaylistAsync(tracks, hoveredPlaylist.Name);
                }
                catch (Exception ex)
                {
                    CoreLogger.Error("Could not add dropped files to hovered playlist. Exception: {0}", ex.Message);
                }
            }
            else if (dropInfo.TargetItem == null)
            {
                string uniquePlaylistName = await this.playlistService.GetUniquePlaylistAsync(ResourceUtils.GetStringResource("Language_New_Playlist"));
                List<string> allFilenames = this.GetDroppedFilenames(dropInfo);
                List<string> audioFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedAudioFile(f)).ToList();
                List<string> playlistFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedPlaylistFile(f)).ToList();

                // 2. Drop audio files in empty part of list: add these files to a new unique playlist
                List<PlayableTrack> audiofileTracks = await this.fileService.ProcessFilesAsync(audioFileNames);

                if (audiofileTracks != null && audiofileTracks.Count > 0)
                {
                    await this.playlistService.AddPlaylistAsync(uniquePlaylistName);
                    await this.playlistService.AddTracksToPlaylistAsync(audiofileTracks, uniquePlaylistName);
                }

                // 3. Drop playlist files in empty part of list: add the playlist with a unique name
                foreach (string playlistFileName in playlistFileNames)
                {
                    uniquePlaylistName = await this.playlistService.GetUniquePlaylistAsync(System.IO.Path.GetFileNameWithoutExtension(playlistFileName));
                    var playlistFileTracks = await this.fileService.ProcessFilesAsync(new string[] { playlistFileName }.ToList());
                    await this.playlistService.AddPlaylistAsync(uniquePlaylistName);
                    await this.playlistService.AddTracksToPlaylistAsync(playlistFileTracks, uniquePlaylistName);
                }
            }
        }
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                // We don't allow dragging playlists
                if (dropInfo.Data is PlaylistViewModel) return;

                // If we're dragging files, we need to be dragging valid files.
                bool isDraggingFiles = this.IsDraggingFiles(dropInfo);
                bool isDraggingValidFiles = false;
                if (isDraggingFiles) isDraggingValidFiles = this.IsDraggingValidFiles(dropInfo);
                if (isDraggingFiles & !isDraggingValidFiles) return;

                // If we're dragging into the list of tracks, there must be playlists, and a playlist must be selected.
                ListBox target = dropInfo.VisualTarget as ListBox;
                if (target.Name.Equals(tracksTarget) && (this.Playlists == null || this.Playlists.Count == 0 || this.SelectedPlaylist == null)) return;

                // If we're dragging tracks into the list of playlists, we cannot drag to the selected playlist.
                string hoveredPlaylistName = null;
                if (dropInfo.TargetItem != null && dropInfo.TargetItem is PlaylistViewModel) hoveredPlaylistName = ((PlaylistViewModel)dropInfo.TargetItem).Name;
                if (!isDraggingFiles && target.Name.Equals(playlistsTarget) && !string.IsNullOrEmpty(hoveredPlaylistName) && !string.IsNullOrEmpty(this.SelectedPlaylistName) && hoveredPlaylistName.Equals(this.SelectedPlaylistName)) return;

                // In all other cases, allow dragging.
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
                dropInfo.NotHandled = true;
            }
            catch (Exception ex)
            {
                dropInfo.NotHandled = false;
                CoreLogger.Error("Could not drag tracks. Exception: {0}", ex.Message);
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            try
            {
                ListBox target = dropInfo.VisualTarget as ListBox;

                if (target.Name.Equals(this.playlistsTarget))
                {
                    // Dragging to the Playlists listbox
                    if (this.IsDraggingFiles(dropInfo))
                    {
                        await this.AddDroppedFilesToPlaylists(dropInfo);
                    }
                    else
                    {
                        await this.AddDroppedTracksToHoveredPlaylist(dropInfo);
                    }
                }
                else if (target.Name.Equals(this.tracksTarget))
                {
                    // Dragging to the Tracks listbox
                    if (this.IsDraggingFiles(dropInfo))
                    {
                        await this.AddDroppedFilesToSelectedPlaylist(dropInfo);
                    }
                    else
                    {
                        GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo); // Automatically performs builtin reorder
                        await this.ReorderSelectedPlaylistTracksAsync(dropInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not perform drop. Exception: {0}", ex.Message);
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
            if (!this.IsFirstLoad()) return;

            await Task.Delay(Constants.CommonListLoadDelay); // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
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
