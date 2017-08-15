using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        Task<List<QueuedTrack>> GetSavedQueuedTracksAsync();
        Task SaveQueuedTracksAsync(IList<QueuedTrack> tracks);
        Task<QueuedTrack> GetPlayingTrackAsync();
    }
}
