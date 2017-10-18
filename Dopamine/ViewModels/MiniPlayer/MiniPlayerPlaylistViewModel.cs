using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Services.Playback;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class MiniPlayerPlaylistViewModel : NowPlayingViewModelBase
    {
        public MiniPlayerPlaylistViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
    }
}
