using Dopamine.Services.Entities;
using System.Collections.Generic;

namespace Dopamine.Services.Playback
{
    public class DequeueResult
    {
        public bool IsSuccess { get; set; }

        public IList<TrackViewModel> DequeuedTracks { get; set; }

        public bool IsPlayingTrackDequeued { get; set; }

        public TrackViewModel NextAvailableTrack { get; set; }
    }
}
