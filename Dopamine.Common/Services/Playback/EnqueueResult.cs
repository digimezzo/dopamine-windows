using Dopamine.Data;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class EnqueueResult
    {
        public bool IsSuccess { get; set; }
        public IList<PlayableTrack> EnqueuedTracks { get; set; }
    }
}
