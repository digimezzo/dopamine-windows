using Dopamine.Common.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface ITrackRepository
    {
        Task<List<PlayableTrack>> GetTracksAsync(IList<string> paths);
        Task<List<PlayableTrack>> GetTracksAsync();
        Task<List<PlayableTrack>> GetTracksAsync(IList<Artist> artists);
        Task<List<PlayableTrack>> GetTracksAsync(IList<Genre> genres);
        Task<List<PlayableTrack>> GetTracksAsync(IList<Album> albums);
        Task<List<PlayableTrack>> GetTracksAsync(IList<Playlist> playlists);
        Track GetTrack(string path);
        Task<Track> GetTrackAsync(string path);
        Task<RemoveTracksResult> RemoveTracksAsync(IList<PlayableTrack> tracks);
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> UpdateTrackFileInformationAsync(string path);
    }
}
