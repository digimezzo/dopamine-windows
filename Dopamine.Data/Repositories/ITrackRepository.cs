using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface ITrackRepository
    {
        Task<List<PlayableTrack>> GetTracksAsync(IList<string> paths);

        Task<List<PlayableTrack>> GetTracksAsync();

        Task<List<PlayableTrack>> GetArtistTracksAsync(IList<Artist> artists);

        Task<List<PlayableTrack>> GetGenreTracksAsync(IList<long> genreIds);

        Task<List<PlayableTrack>> GetAlbumTracksAsync(IList<long> albumIds);

        Track GetTrack(string path);

        Task<Track> GetTrackAsync(string path);

        Task<RemoveTracksResult> RemoveTracksAsync(IList<PlayableTrack> tracks);

        Task<bool> UpdateTrackAsync(Track track);

        Task<bool> UpdateTrackFileInformationAsync(string path);

        Task ClearRemovedTrackAsync();

        Task<IList<string>> GetAllGenresAsync();

        Task<IList<string>> GetAllTrackArtistsAsync();

        Task<IList<string>> GetAllAlbumArtistsAsync();

        Task<IList<AlbumData>> GetAlbumsAsync(IList<string> artists, IList<string> genres);
    }
}
