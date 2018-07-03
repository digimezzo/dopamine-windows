using Dopamine.Services.Entities;
using System.Collections.Generic;

namespace Dopamine.Services.Playback
{
    public class DequeueResult
    {
        public bool IsSuccess { get; set; }
        public List<KeyValuePair<string, TrackViewModel>> DequeuedTracks { get; set; }
        public bool IsPlayingTrackDequeued;
        public KeyValuePair<string, TrackViewModel> NextAvailableTrack;
    }
}
