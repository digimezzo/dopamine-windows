using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Digimezzo.Utilities.Log;
using Dopamine.Core.Helpers;
using Dopamine.Services.Entities;
using Prism.Commands;

namespace Dopamine.Services.Playlist
{
    public abstract class PlaylistServiceBase : IPlaylistServiceBase
    {
        public GentleFolderWatcher Watcher { get; set; }

        public abstract string PlaylistFolder { get; }

        public abstract string DialogFileFilter { get; }

        public event PlaylistAddedHandler PlaylistAdded = delegate { };
        public event PlaylistDeletedHandler PlaylistDeleted = delegate { };
        public event PlaylistRenamedHandler PlaylistRenamed = delegate { };
        public event EventHandler PlaylistFolderChanged = delegate { };

        public DelegateCommand<PlaylistViewModel> DeletePlaylistCommand { get; set; }

        public void OnPlaylistAdded(PlaylistViewModel addedPlaylist)
        {
            this.PlaylistAdded(addedPlaylist);
        }

        public void OnPlaylistDeleted(PlaylistViewModel deletedPlaylist)
        {
            this.PlaylistDeleted(deletedPlaylist);
        }

        public void OnPlaylistRenamed(PlaylistViewModel oldPlaylist, PlaylistViewModel newPlaylist)
        {
            this.PlaylistRenamed(oldPlaylist, newPlaylist);
        }

        public void OnPlaylistFolderChanged(object sender)
        {
            this.PlaylistFolderChanged(sender, new EventArgs());
        }

        public async Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames)
        {
            ImportPlaylistResult finalResult = ImportPlaylistResult.Success;

            foreach (string fileName in fileNames)
            {
                ImportPlaylistResult result = await this.ImportPlaylistAsync(fileName);

                if (!result.Equals(ImportPlaylistResult.Success))
                {
                    finalResult = result;
                }
            }

            return finalResult;
        }

        public async Task<DeletePlaylistsResult> DeletePlaylistAsync(PlaylistViewModel playlist)
        {
            if (playlist == null)
            {
                LogClient.Error($"{nameof(playlist)} is null");
                return DeletePlaylistsResult.Error;
            }

            DeletePlaylistsResult result = DeletePlaylistsResult.Success;

            this.Watcher.Suspend(); // Stop watching the playlist folder

            string filename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(playlist.Path))
                    {
                        System.IO.File.Delete(playlist.Path);
                    }
                    else
                    {
                        result = DeletePlaylistsResult.Error;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while deleting playlist '{0}'. Exception: {1}", playlist.Path, ex.Message);
                    result = DeletePlaylistsResult.Error;
                }
            });

            if (result == DeletePlaylistsResult.Success)
            {
                this.OnPlaylistDeleted(playlist);
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        protected abstract Task<ImportPlaylistResult> ImportPlaylistAsync(string fileName);

        public abstract Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);

        public abstract Task<RenamePlaylistResult> RenamePlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName);
       
        public abstract Task<IList<PlaylistViewModel>> GetPlaylistsAsync();

        public abstract Task<IList<TrackViewModel>> GetTracksAsync(string playlistName);
    }
}
