using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        Task<List<TrackInfo>> GetSavedQueuedTracksAsync();
        Task SaveQueuedTracksAsync(IList<Track> tracks);
    }
}
