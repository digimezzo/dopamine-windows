using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public interface IPlaylistServiceBase
    {
        string PlaylistFolder { get; }

        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);

        Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlistName);

        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);

        Task<IList<PlaylistViewModel>> GetPlaylistsAsync();

        Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames);

        event PlaylistAddedHandler PlaylistAdded;
        event PlaylistDeletedHandler PlaylistDeleted;
        event PlaylistRenamedHandler PlaylistRenamed;
        event EventHandler PlaylistFolderChanged;
    }
}