using Dopamine.Core.Database;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class DequeueResult
    {
        public bool IsSuccess { get; set; }
        public IList<TrackInfo> DequeuedTracks { get; set; }
    }
}
