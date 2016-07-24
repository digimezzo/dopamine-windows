using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface ITrackRepository
    {
        Task<List<TrackInfo>> GetTracksAsync(IList<string> paths);
        Task<List<TrackInfo>> GetTracksAsync();
        Task<List<TrackInfo>> GetTracksAsync(IList<Artist> artists);
        Task<List<TrackInfo>> GetTracksAsync(IList<Genre> genres);
        Task<List<TrackInfo>> GetTracksAsync(IList<Album> albums);
        Task<List<TrackInfo>> GetTracksAsync(IList<Playlist> playlists);
        Task<Track> GetTrackAsync(string path);
        Task<RemoveTracksResult> RemoveTracksAsync(IList<TrackInfo> tracks);
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> UpdateTrackFileInformationAsync(string path);
        Task SaveTrackStatisticsAsync(IList<TrackStatistic> trackStatistics);
    }
}
