using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.ViewModels.Common.Base;
using Dopamine.Views.FullPlayer.Playlists;
using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsSmartPlaylistsViewModel : PlaylistsViewModelBase, IDropTarget
    {
        private IContainerProvider container;
        private ISmartPlaylistService smartPlaylistService;
        private IDialogService dialogService;
        private double leftPaneWidthPercent;

        public PlaylistsSmartPlaylistsViewModel(IContainerProvider container, ISmartPlaylistService smartPlaylistService,
            IDialogService dialogService, IPlaybackService playbackService) : base(container, dialogService, playbackService, smartPlaylistService)
        {
            // Dependency injection
            this.container = container;
            this.smartPlaylistService = smartPlaylistService;
            this.dialogService = dialogService;

            // Commands
            this.NewPlaylistCommand = new DelegateCommand(this.CreateSmartPlaylist);
            this.ImportPlaylistsCommand = new DelegateCommand(async () => await this.ImportPlaylistsAsync());

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "SmartPlaylistsLeftPaneWidthPercent");
        }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "SmartPlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        private async Task ImportDroppedSmartPlaylistFiles(IDropInfo dropInfo)
        {
            IList<string> allFilenames = dropInfo.GetDroppedFilenames();
            IList<string> playlistFileNames = allFilenames.Select(f => f).Where(f => FileFormats.IsSupportedSmartPlaylistFile(f)).ToList();

            await this.ImportPlaylistsAsync(playlistFileNames);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                // We don't allow dragging playlists and tracks 
                if (dropInfo.Data is PlaylistViewModel || dropInfo.Data is TrackViewModel)
                {
                    return;
                }

                // If we're dragging files, we need to be dragging valid files.
                bool isDraggingFiles = dropInfo.IsDraggingFiles();
                bool isDraggingValidFiles = false;

                if (isDraggingFiles)
                {
                    isDraggingValidFiles = dropInfo.IsDraggingSmartPlaylistFiles();
                }

                if (isDraggingFiles & !isDraggingValidFiles)
                {
                    return;
                }

                // We can't drag into the list of tracks.
                ListBox target = dropInfo.VisualTarget as ListBox;

                if (target.Name.Equals(this.TracksTarget))
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
                LogClient.Error("Could not perform drag. Exception: {0}", ex.Message);
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
                    if (dropInfo.IsDraggingSmartPlaylistFiles())
                    {
                        await this.ImportDroppedSmartPlaylistFiles(dropInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not perform drop. Exception: {0}", ex.Message);
            }
        }

        protected override async Task GetPlaylistsAsync()
        {
            try
            {
                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>(await this.smartPlaylistService.GetPlaylistsAsync());

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Smart Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }

            // Notify that the count has changed
            this.RaisePropertyChanged(nameof(this.PlaylistsCount));

            // Select the firts playlist
            this.TrySelectFirstPlaylist();
        }

        protected override async Task DeletePlaylistAsync(PlaylistViewModel playlist)
        {
            DeletePlaylistsResult result = await this.smartPlaylistService.DeletePlaylistAsync(playlist);

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

        private void CreateSmartPlaylist()
        {
            PlaylistsSmartPlaylistsCreate view = this.container.Resolve<PlaylistsSmartPlaylistsCreate>();
            view.DataContext = this.container.Resolve<PlaylistsSmartPlaylistsCreateViewModel>();

            this.dialogService.ShowCustomDialog(
                0xea37,
                16,
                ResourceUtils.GetString("Language_New_Smart_Playlist"),
                view,
                500,
                0,
                false,
                true,
                true,
                true,
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ((PlaylistsSmartPlaylistsCreateViewModel)view.DataContext).SaveSmartPlaylistAsync);
        }
    }
}
