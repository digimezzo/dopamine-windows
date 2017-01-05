using Dopamine.Common.Database;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class DequeueResult
    {
        public bool IsSuccess { get; set; }
        public IList<MergedTrack> DequeuedTracks { get; set; }
    }
}
