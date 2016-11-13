using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        Task<List<MergedTrack>> GetSavedQueuedTracksAsync();
        Task SaveQueuedTracksAsync(IList<string> paths);
    }
}
