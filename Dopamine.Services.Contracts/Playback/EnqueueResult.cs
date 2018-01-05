using Dopamine.Data.Contracts.Entities;
using System.Collections.Generic;

namespace Dopamine.Services.Contracts.Playback
{
    public class EnqueueResult
    {
        public bool IsSuccess { get; set; }
        public IList<PlayableTrack> EnqueuedTracks { get; set; }
    }
}
