using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface ITrackRepository
    {
        Task<List<MergedTrack>> GetMergedTracksAsync(IList<string> paths);
        Task<List<MergedTrack>> GetMergedTracksAsync();
        Task<List<MergedTrack>> GetMergedTracksAsync(IList<Artist> artists);
        Task<List<MergedTrack>> GetMergedTracksAsync(IList<Genre> genres);
        Task<List<MergedTrack>> GetMergedTracksAsync(IList<Album> albums);
        Task<List<MergedTrack>> GetMergedTracksAsync(IList<Playlist> playlists);
        Task<Track> GetTrackAsync(string path);
        Task<MergedTrack> GetMergedTrackAsync(string path);
        Task<RemoveTracksResult> RemoveTracksAsync(IList<string> paths);
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> UpdateTrackFileInformationAsync(string path);
        Task SaveTrackStatisticsAsync(IList<TrackStatistic> trackStatistics);
    }
}
