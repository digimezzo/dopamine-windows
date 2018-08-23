using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public delegate void PlaylistAddedHandler(PlaylistViewModel addedPlaylist);
    public delegate void TracksAddedHandler(int numberTracksAdded, string playlistName);
    public delegate void TracksDeletedHandler(string playlistName);
    public delegate void PlaylistDeletedHandler(PlaylistViewModel deletedPlaylist);
    public delegate void PlaylistRenamedHandler(PlaylistViewModel oldPLaylist, PlaylistViewModel newPlaylist);

    public interface IPlaylistService
    {
        string PlaylistFolder { get; }

        Task<string> GetUniquePlaylistAsync(string proposedPlaylistName);

        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);

        Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlistName);

        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);

        Task<IList<PlaylistViewModel>> GetPlaylistsAsync();

        Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames);

        Task<IList<TrackViewModel>> GetTracks(string playlistName);

        Task SetPlaylistOrderAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<string> artists, string playlistName);

        Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<string> genres, string playlistName);

        Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName);

        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromPlaylistAsync(IList<int> indexes, string playlistName);

        event PlaylistAddedHandler PlaylistAdded;
        event PlaylistDeletedHandler PlaylistDeleted;
        event PlaylistRenamedHandler PlaylistRenamed;
        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
        event EventHandler PlaylistFolderChanged;
    }
}
