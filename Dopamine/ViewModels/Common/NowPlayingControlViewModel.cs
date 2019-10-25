using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.File;
using Dopamine.Services.Playback;
using Dopamine.ViewModels.Common.Base;
using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common
{
    public class NowPlayingControlViewModel : TracksViewModelBase, IDropTarget
    {
        private IPlaybackService playbackService;
        private IDialogService dialogService;
        private IFileService fileService;
        protected bool isDroppingTracks;

        public NowPlayingControlViewModel(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.playbackService = container.Resolve<IPlaybackService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.fileService = container.Resolve<IFileService>();

            // Commands
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await RemoveSelectedTracksFromNowPlayingAsync());
        }

        protected async Task GetTracksAsync()
        {
            await this.GetTracksCommonAsync(this.playbackService.Queue, TrackOrder.None);
        }

        protected override async Task FillListsAsync()
        {
            // Not implemented here. We use our own LoadedCommandAsync here, because we need our own delay and tracks source.
        }

        protected async override Task EmptyListsAsync()
        {
            cthis.ClearTracks();
        }

        protected async override Task LoadedCommandAsync()
        {
            // Wait for the UI to slide in
            await Task.Delay(Constants.NowPlayingListLoadDelay);  

            // If there is a queue, get the tracks.
            if (this.playbackService.HasQueue)
            {
                await this.GetTracksAsync();
            }

            // Listen to queue changes.
            this.playbackService.QueueChanged += async (_, __) =>
            {
                if (!this.isDroppingTracks)
                {
                    await this.GetTracksAsync();
                }
            };
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.DragOver(dropInfo);

            try
            {
                // We don't allow dragging playlists
                if (dropInfo.Data is PlaylistViewModel) return;

                // If we're dragging files, we need to be dragging valid files.
                bool isDraggingFiles = dropInfo.IsDraggingFilesOrDirectories();
                bool isDraggingValidFiles = false;
                if (isDraggingFiles) isDraggingValidFiles = dropInfo.IsDraggingMediaFiles();
                if (isDraggingFiles & !isDraggingValidFiles) return;

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

        private async Task UpdateQueueOrderAsync(IDropInfo dropInfo)
        {
            this.isDroppingTracks = true;

            var droppedTracks = new List<TrackViewModel>();

            // TargetCollection contains all tracks of the queue, in the new order.
            foreach (var item in dropInfo.TargetCollection)
            {
                droppedTracks.Add((TrackViewModel)item);
            }

            await this.playbackService.UpdateQueueOrderAsync(droppedTracks);

            this.isDroppingTracks = false;
        }

        public async void Drop(IDropInfo dropInfo)
        {
            try
            {
                if (dropInfo.IsDraggingFilesOrDirectories())
                {
                    if (dropInfo.IsDraggingMediaFiles())
                    {
                        await this.AddDroppedFilesToQueue(dropInfo);
                    }
                }
                else
                {
                    DragDrop.DefaultDropHandler.Drop(dropInfo); // Automatically performs built-in reorder
                    await this.UpdateQueueOrderAsync(dropInfo);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not perform drop. Exception: {0}", ex.Message);
            }
        }

        private async Task AddDroppedFilesToQueue(IDropInfo dropInfo)
        {
            try
            {
                IList<string> filenames = dropInfo.GetDroppedFilenames();
                IList<TrackViewModel> tracks = await this.fileService.ProcessFilesAsync(filenames, true);
                await this.playbackService.AddToQueueAsync(tracks);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not add dropped files to playback queue. Exception: {0}", ex.Message);
            }
        }

        private async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            // Remove Tracks from PlaybackService (this dequeues the Tracks)
            DequeueResult dequeueResult = await this.playbackService.DequeueAsync(this.SelectedTracks);

            if (!dequeueResult.IsSuccess)
            {
                this.dialogService.ShowNotification(
                     0xe711,
                     16,
                     ResourceUtils.GetString("Language_Error"),
                     ResourceUtils.GetString("Language_Error_Removing_From_Now_Playing"),
                     ResourceUtils.GetString("Language_Ok"),
                     true,
                     ResourceUtils.GetString("Language_Log_File"));
            }

            if (this.Tracks == null)
            {
                return;
            }

            // Remove the ViewModels from Tracks (this updates the UI)
            foreach (TrackViewModel track in dequeueResult.DequeuedTracks)
            {
                if (this.Tracks.Contains(track))
                {
                    this.Tracks.Remove(track);
                }
            }

            this.TracksCount = this.Tracks.Count;
        }
    }
}
