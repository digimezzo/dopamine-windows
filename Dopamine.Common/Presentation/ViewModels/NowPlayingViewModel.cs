using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Prism;
using Dopamine.Core.Utils;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class NowPlayingViewModel : CommonTracksViewModel
    {
        #region Variables
        private bool allowFillAllLists = true;
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
        public NowPlayingViewModel() : base()
        {
            // Commands
            this.RemoveFromNowPlayingCommand = new DelegateCommand(async () => await RemoveSelectedTracksFromNowPlayingAsync());

            // Events
            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe((enableRating) => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe((enableLove) => this.EnableLove = enableLove);

            // PlaybackService
            this.playbackService.ShuffledTracksChanged += async (_, __) =>
            {
                if (this.allowFillAllLists)
                {
                    await this.FillListsAsync();
                }
            };
        }
        #endregion

        #region Protected
        protected async Task GetTracksAsync()
        {
            var tracks = await this.trackRepository.GetTracksAsync(this.playbackService.Queue);
            await this.GetTracksCommonAsync(tracks, TrackOrder.None);
        }

        protected override void ShowPlayingTrackAsync()
        {
            if (this.playbackService.PlayingFile == null) return;

            base.ShowPlayingTrackAsync();
        }
        #endregion

        #region Private
        public async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            this.allowFillAllLists = false;

            // Remove TrackInfos from PlaybackService (this dequeues the Tracks)
            DequeueResult dequeueResult = await this.playbackService.Dequeue(this.SelectedTracks);

            var trackInfoViewModelsToRemove = new List<TrackInfoViewModel>();

            await Task.Run(() =>
            {
                // Collect the TrackInfoViewModels to remove
                foreach (TrackInfoViewModel tivm in this.Tracks)
                {
                    if (dequeueResult.DequeuedFiles.Contains(tivm.TrackInfo.Path))
                    {
                        trackInfoViewModelsToRemove.Add(tivm);
                    }
                }
            });

            // Remove the TrackInfoViewModels from Tracks (this updates the UI)
            foreach (TrackInfoViewModel tivm in trackInfoViewModelsToRemove)
            {
                this.Tracks.Remove(tivm);
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
    }
}
