using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Helpers;
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
        Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<MergedTrack> mergedTracks, string playlistName);
        Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string iPlaylistName);
        Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<MergedTrack> mergedTracks, Playlist selectedPlaylist);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);
        Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> playlists);
        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);
        Task<List<Playlist>> GetPlaylistsAsync();
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<string> selectedPaths);
        Task<OpenPlaylistResult> OpenPlaylistAsync(string iFileName);
        Task SetTrackArtworkAsync(ObservableCollection<MergedTrackViewModel> mergedTrackViewModels, int delayMilliSeconds);
        Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewmodels, int delayMilliSeconds);
        Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, ObservableCollection<MergedTrackViewModel> mergedTrackViewModels);
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
