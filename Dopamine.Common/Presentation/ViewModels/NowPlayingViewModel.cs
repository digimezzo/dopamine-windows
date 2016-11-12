using Dopamine.Common.Services.Metadata;
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
                if (this.allowFillAllLists) await this.FillListsAsync();
            };

            // MetadataService
            this.metadataService.MetadataChanged += MetadataService_MetadataChanged;
        }

        private async void MetadataService_MetadataChanged(MetadataChangedEventArgs e)
        {
            bool refreshTracks = false;

            await Task.Run(() =>
            {
                List<string> paths = this.playbackService.Queue;

                foreach (string path in paths)
                {
                    if (e.ChangedPaths.Contains(path))
                    {
                        refreshTracks = true;
                        break;
                    }
                }
            });

            if (refreshTracks) await this.FillListsAsync();
        }
        #endregion

        #region Protected
        protected async Task GetTracksAsync()
        {
            var mergedTracks = await this.trackRepository.GetMergedTracksAsync(this.playbackService.Queue);
            await this.GetTracksCommonAsync(mergedTracks, TrackOrder.None);
        }

        protected override void ShowPlayingTrackAsync()
        {
            if (this.playbackService.PlayingPath == null) return;

            base.ShowPlayingTrackAsync();
        }
        #endregion

        #region Private
        public async Task RemoveSelectedTracksFromNowPlayingAsync()
        {
            this.allowFillAllLists = false;

            // Dequeue paths from PlaybackService
            DequeueResult dequeueResult = await this.playbackService.Dequeue(this.SelectedTracks.Select(t => t.Path).ToList());

            var mergedTrackViewModelsToRemove = new List<MergedTrackViewModel>();

            await Task.Run(() =>
            {
                // Collect the ViewModels to remove
                foreach (MergedTrackViewModel vm in this.Tracks)
                {
                    if (dequeueResult.DequeuedPaths.Contains(vm.MergedTrack.Path))
                    {
                        mergedTrackViewModelsToRemove.Add(vm);
                    }
                }
            });

            // Remove the ViewModels from Tracks (this updates the UI)
            foreach (MergedTrackViewModel vm in mergedTrackViewModelsToRemove)
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
