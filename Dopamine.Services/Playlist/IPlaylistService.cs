using Dopamine.Services.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public interface IPlaylistService : IPlaylistServiceBase
    {
        Task<string> GetUniquePlaylistAsync(string proposedPlaylistName);

        Task<IList<TrackViewModel>> GetTracks(string playlistName);

        Task SetPlaylistOrderAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<string> artists, string playlistName);

        Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<string> genres, string playlistName);

        Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName);

        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromPlaylistAsync(IList<int> indexes, string playlistName);

        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
    }
}
