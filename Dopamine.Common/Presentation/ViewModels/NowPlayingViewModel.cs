using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Dopamine.Core.Utils;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class NowPlayingViewModel : CommonTracksViewModel, IDropTarget
    {
        #region Variables
        private bool allowFillAllLists = true;
        private bool isDroppingTracks;
        #endregion

        #region Commands
        public DelegateCommand RemoveFromNowPlayingCommand { get; set; }
        #endregion

        #region Properties
        public override bool CanOrderByAlbum
        {
            get { return false; } // Doesn't need to return a useful value in this class
        }
        #endregion

        #region Construction
        public NowPlayingViewModel(IUnityContainer container) : base(container)
        {
            // Commands
            this.RemoveFromNowPlayingCommand = new DelegateCommand(async () => await RemoveSelectedTracksFromNowPlayingAsync());

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe((enableRating) => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe((enableLove) => this.EnableLove = enableLove);

            // PlaybackService
            this.playbackService.QueueChanged += async (_, __) => { if (!isDroppingTracks) await this.FillListsAsync(); };
        }
        #endregion

        #region Protected
        protected async Task GetTracksAsync()
        {
            await this.GetTracksCommonAsync(this.playbackService.Queue, TrackOrder.None);
        }

        protected override void ShowPlayingTrackAsync()
        {
            if (this.playbackService.PlayingTrack == null)
            {
                return;
            }

            base.ShowPlayingTrackAsync();
        }
        #endregion

        #region Private
        public async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            this.allowFillAllLists = false;

            // Remove Tracks from PlaybackService (this dequeues the Tracks)
            DequeueResult dequeueResult = await this.playbackService.Dequeue(this.SelectedTracks);

            var viewModelsToRemove = new List<MergedTrackViewModel>();

            await Task.Run(() =>
            {
                // Collect the ViewModels to remove
                foreach (MergedTrackViewModel vm in this.Tracks)
                {
                    if (dequeueResult.DequeuedTracks.Select((t) => t.Path).ToList().Contains(vm.Track.Path))
                    {
                        viewModelsToRemove.Add(vm);
                    }
                }
            });

            // Remove the ViewModels from Tracks (this updates the UI)
            foreach (MergedTrackViewModel vm in viewModelsToRemove)
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

            this.allowFillAllLists = true;
        }
        #endregion

        #region Overrides
        protected override async Task FillListsAsync()
        {
            if (!this.allowFillAllLists) return;
            await this.GetTracksAsync();
        }

        protected override void Subscribe()
        {
            // Do Nothing
        }

        protected override void Unsubscribe()
        {
            // Do Nothing
        }

        protected override void RefreshLanguage()
        {
            // Do Nothing
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
                LogClient.Instance.Logger.Error("Could not drag tracks. Exception: {0}", ex.Message);
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            isDroppingTracks = true;

            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            try
            {
                var tracks = new List<MergedTrack>();

                foreach (var item in dropInfo.TargetCollection)
                {
                    tracks.Add(((MergedTrackViewModel)item).Track);
                }

                await this.playbackService.UpdateQueueOrderAsync(tracks);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not drop tracks. Exception: {0}", ex.Message);
            }

            isDroppingTracks = false;
        }
        #endregion
    }
}
