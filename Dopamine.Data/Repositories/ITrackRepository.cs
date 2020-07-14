using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface ITrackRepository
    {
        Task<List<Track>> GetTracksAsync(IList<string> paths);

        Task<List<Track>> GetTracksAsync();

        Task<List<Track>> GetTracksAsync(string whereClause);

        Task<List<Track>> GetArtistTracksAsync(IList<string> artistNames);

        Task<List<Track>> GetGenreTracksAsync(IList<string> genreNames);

        Task<List<Track>> GetAlbumTracksAsync(IList<string> albumKeys);

        Track GetTrack(string path);

        Task<Track> GetTrackAsync(string path);

        Task<RemoveTracksResult> RemoveTracksAsync(IList<Track> tracks);

        Task<bool> UpdateTrackAsync(Track track);

        Task<bool> UpdateTrackFileInformationAsync(string path);

        Task ClearRemovedTrackAsync();

        Task<IList<string>> GetTrackArtistsAsync();

        Task<IList<string>> GetAlbumArtistsAsync();

        Task<IList<string>> GetGenresAsync();

        Task<IList<AlbumData>> GetArtistAlbumDataAsync(IList<string> artists, ArtistType artistType);

        Task<IList<AlbumData>> GetGenreAlbumDataAsync(IList<string> genres);

        Task<IList<AlbumData>> GetAllAlbumDataAsync();

        Task<AlbumData> GetAlbumDataAsync(string albumKey);

        Task<IList<AlbumData>> GetAlbumDataToIndexAsync();

        Task<Track> GetLastModifiedTrackForAlbumKeyAsync(string albumKey);

        Task DisableNeedsAlbumArtworkIndexingAsync(string albumKey);

        Task DisableNeedsAlbumArtworkIndexingForAllTracksAsync();

        Task EnableNeedsAlbumArtworkIndexingForAllTracksAsync(bool onlyWhenHasNoCover);

        Task UpdateRatingAsync(string path, int rating);

        Task UpdateLoveAsync(string path, int love);

        Task UpdatePlaybackCountersAsync(PlaybackCounter counters);

        Task<PlaybackCounter> GetPlaybackCountersAsync(string path);
    }
}
