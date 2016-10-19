using Dopamine.Common.Services.Playback;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region Construction
        public NowPlayingPlaybackControlsViewModel(IPlaybackService playbackService) : base(playbackService)
        {
            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
            };
        }
        #endregion
    }
}
