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
        Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> iArtists, string iPlaylistName);
        Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> iGenres, string iPlaylistName);
        Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackInfo> iTracks, string iPlaylistName);
        Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> iAlbums, string iPlaylistName);
        Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<TrackInfo> iTracks, Playlist iSelectedPlaylist);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string iOldPlaylistName, string iNewPlaylistName);
        Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> iPlaylists);
        Task<AddPlaylistResult> AddPlaylistAsync(string iPlaylistName);
        Task<List<Playlist>> GetPlaylistsAsync();
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<TrackInfo> iSelectedTracks);
        Task<OpenPlaylistResult> OpenPlaylistAsync(string iFileName);
        Task SetTrackArtworkAsync(ObservableCollection<TrackInfoViewModel> iTrackInfoViewModels, int iDelayMilliSeconds);
        Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> iAlbumViewmodels, int iDelayMilliSeconds);
        Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> iAlbumViewModels, ObservableCollection<TrackInfoViewModel> iTrackInfoViewModels);
        Task<ExportPlaylistsResult> ExportPlaylistsAsync(IList<Playlist> iPlaylists, string iDestinationDirectory);
        Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist iPlaylist, string iFullPlaylistPath, bool iGenerateUniqueName);
        Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist iPlaylist, string iDestinationDirectory, string iPlaylistName, bool iGenerateUniqueName);
        Task MarkFolderAsync(Folder iFolder);
        Task SaveMarkedFoldersAsync();

        event EventHandler CollectionChanged;
        event EventHandler PlaylistsChanged;
        event Action<int,string> AddedTracksToPlaylist;
        event EventHandler DeletedTracksFromPlaylists;
    }
}
