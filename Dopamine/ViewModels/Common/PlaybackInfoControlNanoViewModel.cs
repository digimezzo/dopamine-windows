using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Playback;

namespace Dopamine.ViewModels.Common
{
    public class PlaybackInfoControlNanoViewModel : PlaybackInfoControlViewModel
    {
        public PlaybackInfoControlNanoViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
    }
}