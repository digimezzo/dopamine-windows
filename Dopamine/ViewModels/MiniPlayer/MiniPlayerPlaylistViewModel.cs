using Dopamine.ViewModels.Common.Base;
using Dopamine.Services.Playback;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class MiniPlayerPlaylistViewModel : NowPlayingViewModelBase
    {
        public MiniPlayerPlaylistViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
    }
}
