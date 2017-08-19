using Dopamine.Core.Logging;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class NowPlayingViewModelBase : PlaylistViewModelBase, IDropTarget
    {
        #region Variables
        private IPlaybackService playbackService;
        private IDialogService dialogService;
        #endregion

        #region Commands
        public DelegateCommand RemoveFromNowPlayingCommand { get; set; }
        #endregion

        #region Construction
        public NowPlayingViewModelBase(IUnityContainer container)
           : base(container)
        {
            // Dependency injection
            this.playbackService = container.Resolve<IPlaybackService>();
            this.dialogService = container.Resolve<IDialogService>();

            // Commands
            this.RemoveFromNowPlayingCommand = new DelegateCommand(async () => await RemoveSelectedTracksFromNowPlayingAsync());

            // PlaybackService
            this.playbackService.QueueChanged += async (_, __) => { if (!isDroppingTracks) await this.FillListsAsync(); };
        }
        #endregion

        #region Protected
        protected async Task GetTracksAsync()
        {
            await this.GetTracksCommonAsync(this.playbackService.Queue);
        }
        #endregion

        #region Overrides
        protected override async Task FillListsAsync()
        {
            await this.GetTracksAsync();
        }

        protected override async Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);

            try
            {
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
            isDroppingTracks = true;

            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            try
            {
                var droppedTracks = new List<KeyValuePair<string, PlayableTrack>>();

                foreach (var item in dropInfo.TargetCollection)
                {
                    KeyValuePair<string, TrackViewModel> droppedItem = (KeyValuePair<string, TrackViewModel>)item;
                    droppedTracks.Add(new KeyValuePair<string, PlayableTrack>(droppedItem.Key, droppedItem.Value.Track));
                }

                await this.playbackService.UpdateQueueOrderAsync(droppedTracks);
            }
            catch (Exception ex)
            {
                CoreLogger.Error("Could not drop tracks. Exception: {0}", ex.Message);
            }

            isDroppingTracks = false;
        }
        #endregion

        #region Private
        private async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            // Remove Tracks from PlaybackService (this dequeues the Tracks)
            DequeueResult dequeueResult = await this.playbackService.DequeueAsync(this.SelectedTracks);

            var viewModelsToRemove = new List<KeyValuePair<string, TrackViewModel>>();

            await Task.Run(() =>
            {
                // Collect the ViewModels to remove
                viewModelsToRemove.AddRange(this.Tracks.Where(vm => dequeueResult.DequeuedTracks.Select(t => t.Key)
                    .ToList()
                    .Contains(vm.Key)));
            });

            // Remove the ViewModels from Tracks (this updates the UI)
            foreach (KeyValuePair<string, TrackViewModel> vm in viewModelsToRemove)
            {
                this.Tracks.Remove(vm);
            }

            this.TracksCount = this.Tracks.Count;

            if (!dequeueResult.IsSuccess)
            {
                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetStringResource("Language_Error"),
                    ResourceUtils.GetStringResource("Language_Error_Removing_From_Now_Playing"),
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }
        #endregion
    }
}
