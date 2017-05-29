using Dopamine.Common.Services.Playback;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackInfoControlNanoViewModel : PlaybackInfoControlViewModel
    {
        #region Construction

        public PlaybackInfoControlNanoViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
        #endregion
    }
}