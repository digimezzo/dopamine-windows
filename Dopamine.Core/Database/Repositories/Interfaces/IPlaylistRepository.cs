using Dopamine.Core.Database.Entities;
using Dopamine.Core.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<List<Playlist>> GetPlaylistsAsync();
        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);
        Task<Playlist> GetPlaylistAsync(string playlistName);
        Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> playlists);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);
        Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackInfo> tracks, string playlistName);
        Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName);
        Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName);
        Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName);
        Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<TrackInfo> tracks, Playlist selectedPlaylist);
        Task<string> GetUniquePlaylistNameAsync(string name);
    }
}
