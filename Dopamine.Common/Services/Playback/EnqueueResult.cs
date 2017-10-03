using Dopamine.Common.Database;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class EnqueueResult
    {
        public bool IsSuccess { get; set; }
        public IList<PlayableTrack> EnqueuedTracks { get; set; }
    }
}
