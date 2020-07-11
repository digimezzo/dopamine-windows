using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IQueuedTrackRepository
    {
        Task<List<Track>> GetSavedQueuedTracksAsync();
        Task SaveQueuedTracksAsync(IList<Track> tracks, long? currentTrackId, long progressSeconds);
        Task<Tuple<Track, long>> GetPlayingTrackAsync();
    }
}
