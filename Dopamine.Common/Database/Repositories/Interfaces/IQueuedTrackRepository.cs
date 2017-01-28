using Dopamine.Common.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        List<QueuedTrack> GetSavedQueuedTracks();
        Task SaveQueuedTracksAsync(IList<QueuedTrack> tracks);
        Task<QueuedTrack> GetPlayingTrackAsync();
    }
}
