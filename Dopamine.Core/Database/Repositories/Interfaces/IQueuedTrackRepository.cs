using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        List<MergedTrack> GetSavedQueuedTracks();
        Task SaveQueuedTracksAsync(IList<string> paths);
    }
}
