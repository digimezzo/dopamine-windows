using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Playback;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackInfoControlNanoViewModel : PlaybackInfoControlViewModel
    {
        public PlaybackInfoControlNanoViewModel(IPlaybackService playbackService) : base(playbackService)
        {
        }
    }
}