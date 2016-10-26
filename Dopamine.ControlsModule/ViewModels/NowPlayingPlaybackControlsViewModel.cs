using Dopamine.Common.Services.Playback;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region Properties
        public bool HasPlaybackQueue
        {
            get { return this.playbackService.Queue.Count > 0; }
        }

        #endregion

        #region Construction
        public NowPlayingPlaybackControlsViewModel(IPlaybackService playbackService) : base(playbackService)
        {
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.HasPlaybackQueue);

            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
            };
        }
        #endregion
    }
}
