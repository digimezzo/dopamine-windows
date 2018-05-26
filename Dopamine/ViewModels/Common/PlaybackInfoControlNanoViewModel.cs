using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Dopamine.Services.Scrobbling;

namespace Dopamine.ViewModels.Common
{
    public class PlaybackInfoControlNanoViewModel : PlaybackInfoControlViewModel
    {
        public PlaybackInfoControlNanoViewModel(
            IPlaybackService playbackService, 
            IMetadataService metadataService,
            IScrobblingService scrobblingService) : base(
            playbackService, 
            metadataService,
            scrobblingService
            )
        {
        }
    }
}