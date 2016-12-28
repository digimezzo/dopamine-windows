using Dopamine.Common.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IQueuedTrackRepository
    {
        List<MergedTrack> GetSavedQueuedTracks();
        Task SaveQueuedTracksAsync(IList<string> paths, string playingPath, double progressSeconds);
        Task<QueuedTrack> GetPlayingTrackAsync();
    }
}
