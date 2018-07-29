using Dopamine.Services.Entities;
using System.Collections.Generic;

namespace Dopamine.Services.Playback
{
    public class EnqueueResult
    {
        public bool IsSuccess { get; set; }

        public IList<TrackViewModel> EnqueuedTracks { get; set; }
    }
}
