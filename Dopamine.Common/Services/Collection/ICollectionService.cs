using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Collection
{
    public interface ICollectionService
    {
        Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName);
        Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName);
        Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<MergedTrack> tracks, string playlistName);
        Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName);
        Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<MergedTrack> tracks, Playlist selectedPlaylist);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);
        Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> playlists);
        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);
        Task<List<Playlist>> GetPlaylistsAsync();
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<MergedTrack> selectedTracks);
        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<MergedTrack> selectedTracks);
        Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName);
        Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewmodels, int delayMilliSeconds);
        Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels);
        Task<ExportPlaylistsResult> ExportPlaylistsAsync(IList<Playlist> playlists, string destinationDirectory);
        Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string fullPlaylistPath, bool generateUniqueName);
        Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string destinationDirectory, string playlistName, bool generateUniqueName);
        Task MarkFolderAsync(Folder folder);
        Task SaveMarkedFoldersAsync();

        event EventHandler CollectionChanged;
        event EventHandler PlaylistsChanged;
        event Action<int,string> AddedTracksToPlaylist;
        event EventHandler DeletedTracksFromPlaylists;
    }
}
