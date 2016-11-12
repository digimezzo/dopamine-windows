using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        Task<List<string>> GetSavedQueuedPathsAsync();
        Task SaveQueuedPathsAsync(IList<string> paths);
    }
}
