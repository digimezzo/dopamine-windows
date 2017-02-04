using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playlist
{
    public interface IPlaylistService
    {
        Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlist);
        Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlist);
        Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlist);
        Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlist);
        Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<PlayableTrack> tracks, string playlist);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldplaylist, string newplaylist);
        Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<string> playlists);
        Task<AddPlaylistResult> AddPlaylistAsync(string playlist);
        Task<List<string>> GetPlaylistsAsync();
        Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName);
        Task<ExportPlaylistsResult> ExportPlaylistsAsync(IList<string> playlists, string destinationDirectory);
        Task<ExportPlaylistsResult> ExportPlaylistAsync(string playlist, string destinationDirectory, bool generateUniqueName);
        event EventHandler PlaylistsChanged;
        event Action<int, string> AddedTracksToPlaylist;
        event EventHandler DeletedTracksFromPlaylists;
    }
}
