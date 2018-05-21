using Dopamine.Data;
using Dopamine.Data.Entities;
using System.Collections.Generic;

namespace Dopamine.Services.Playback
{
    public class DequeueResult
    {
        public bool IsSuccess { get; set; }
        public List<KeyValuePair<string, PlayableTrack>> DequeuedTracks { get; set; }
        public bool IsPlayingTrackDequeued;
        public KeyValuePair<string, PlayableTrack> NextAvailableTrack;
    }
}
