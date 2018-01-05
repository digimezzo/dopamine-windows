using Dopamine.Data.Contracts.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Contracts.Repositories
{
    public interface IQueuedTrackRepository
    {
        Task<List<QueuedTrack>> GetSavedQueuedTracksAsync();
        Task SaveQueuedTracksAsync(IList<QueuedTrack> tracks);
        Task<QueuedTrack> GetPlayingTrackAsync();
    }
}
