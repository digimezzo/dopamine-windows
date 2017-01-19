using Dopamine.Common.Database;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class AddToQueueResult
    {
        public bool IsSuccess { get; set; }
        public IList<PlayableTrack> AddedTracks { get; set; }
    }
}
