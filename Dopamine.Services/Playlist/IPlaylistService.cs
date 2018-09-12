using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public interface IPlaylistService
    {
        string PlaylistFolder { get; }

        string DialogFileFilter { get; }

        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
        event EventHandler PlaylistFolderChanged;

        Task<CreateNewPlaylistResult> CreateNewPlaylistAsync(string playlistName, PlaylistType type);

        Task<AddTracksToPlaylistResult> AddTracksToStaticPlaylistAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddArtistsToStaticPlaylistAsync(IList<string> artists, string playlistName);

        Task<AddTracksToPlaylistResult> AddGenresToStaticPlaylistAsync(IList<string> genres, string playlistName);

        Task<AddTracksToPlaylistResult> AddAlbumsToStaticPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName);

        Task<IList<PlaylistViewModel>> GetStaticPlaylistsAsync();

        Task<IList<PlaylistViewModel>> GetAllPlaylistsAsync();

        Task<IList<TrackViewModel>> GetTracksAsync(PlaylistViewModel playlist);

        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromStaticPlaylistAsync(IList<int> indexes, PlaylistViewModel playlist);

        Task<string> GetUniquePlaylistNameAsync(string proposedPlaylistName);

        Task<RenamePlaylistResult> RenamePlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName);

        Task<DeletePlaylistsResult> DeletePlaylistAsync(PlaylistViewModel playlist);

        Task SetStaticPlaylistOrderAsync(PlaylistViewModel playlist, IList<TrackViewModel> tracks);

        Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames);
    }
}
