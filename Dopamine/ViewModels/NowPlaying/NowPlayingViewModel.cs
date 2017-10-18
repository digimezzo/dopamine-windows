using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Services.Playback;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingViewModel : NowPlayingViewModelBase
    {
        public NowPlayingViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
    }
}
