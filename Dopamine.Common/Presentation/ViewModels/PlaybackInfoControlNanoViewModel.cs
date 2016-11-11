using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database.Repositories.Interfaces;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackInfoControlNanoViewModel : PlaybackInfoControlViewModel
    {
        #region Construction

        public PlaybackInfoControlNanoViewModel(IPlaybackService playbackService, ITrackRepository trackRepository) : base(playbackService, trackRepository)
        {
        }
        #endregion
    }
}
