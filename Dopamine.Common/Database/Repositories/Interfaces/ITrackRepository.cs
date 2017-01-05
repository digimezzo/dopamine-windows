using Dopamine.Common.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface ITrackRepository
    {
        Task<List<MergedTrack>> GetTracksAsync(IList<string> paths);
        Task<List<MergedTrack>> GetTracksAsync();
        Task<List<MergedTrack>> GetTracksAsync(IList<Artist> artists);
        Task<List<MergedTrack>> GetTracksAsync(IList<Genre> genres);
        Task<List<MergedTrack>> GetTracksAsync(IList<Album> albums);
        Task<List<MergedTrack>> GetTracksAsync(IList<Playlist> playlists);
        Track GetTrack(string path);
        Task<Track> GetTrackAsync(string path);
        Task<RemoveTracksResult> RemoveTracksAsync(IList<MergedTrack> tracks);
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> UpdateTrackFileInformationAsync(string path);
        Task SaveTrackStatisticsAsync(IList<TrackStatistic> trackStatistics);
    }
}
