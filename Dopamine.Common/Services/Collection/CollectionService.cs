using Digimezzo.Utilities.Utils;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Dopamine.Common.IO;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dopamine.Common.Services.Playback;

namespace Dopamine.Common.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        #region Variables
        private IPlaylistRepository playlistRepository;
        private IAlbumRepository albumRepository;
        private IArtistRepository artistRepository;
        private ITrackRepository trackRepository;
        private IGenreRepository genreRepository;
        private IFolderRepository folderRepository;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private List<Folder> markedFolders;
        #endregion

        #region Construction
        public CollectionService(IPlaylistRepository playlistRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, ITrackRepository trackRepository, IGenreRepository genreRepository, IFolderRepository folderRepository, ICacheService cacheService, IPlaybackService playbackService)
        {
            this.playlistRepository = playlistRepository;
            this.albumRepository = albumRepository;
            this.artistRepository = artistRepository;
            this.trackRepository = trackRepository;
            this.genreRepository = genreRepository;
            this.folderRepository = folderRepository;
            this.cacheService = cacheService;
            this.playbackService = playbackService;
            this.markedFolders = new List<Folder>();
        }
        #endregion

        #region Events
        public event EventHandler CollectionChanged = delegate { };
        public event EventHandler PlaylistsChanged = delegate { };
        public event Action<int, string> AddedTracksToPlaylist = delegate { };
        public event EventHandler DeletedTracksFromPlaylists = delegate { };
        #endregion

        #region ICollectionService
        public async Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName)
        {
            List<MergedTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            List<MergedTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<MergedTrack> tracks, string playlistName)
        {
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName)
        {
            List<MergedTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<MergedTrack> tracks, Playlist selectedPlaylist)
        {
            DeleteTracksFromPlaylistsResult result = await this.playlistRepository.DeleteTracksFromPlaylistAsync(tracks, selectedPlaylist);

            if (result == DeleteTracksFromPlaylistsResult.Success)
            {
                this.DeletedTracksFromPlaylists(this, new EventArgs());
            }

            return result;
        }

        public async Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<MergedTrack> selectedTracks)
        {
            RemoveTracksResult result = await this.trackRepository.RemoveTracksAsync(selectedTracks);

            if (result == RemoveTracksResult.Success)
            {
                // Delete orphaned Albums
                await this.albumRepository.DeleteOrphanedAlbumsAsync();

                // Delete orphaned Artists
                await this.artistRepository.DeleteOrphanedArtistsAsync();

                // Delete orphaned Genres
                await this.genreRepository.DeleteOrphanedGenresAsync();

                this.CollectionChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<MergedTrack> selectedTracks)
        {
            RemoveTracksResult result = await this.trackRepository.RemoveTracksAsync(selectedTracks);

            if (result == RemoveTracksResult.Success)
            {
                // If result is Success: we can assume that all selected tracks were removed from the collection,
                // as this happens in a transaction in trackRepository. If removing 1 or more tracks fails, the
                // transaction is rolled back and no tracks are removed.
                foreach (var track in selectedTracks)
                {
                    // When the track is playing, the corresponding file is handled by the CSCore.
                    // To delete the file properly, PlaybackService must release this handle.
                    await this.playbackService.StopIfPlayingAsync(track);
                    
                    // Delete file from disk
                    FileUtils.SendToRecycleBinSilent(track.Path);
                }

                this.CollectionChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            RenamePlaylistResult result = await this.playlistRepository.RenamePlaylistAsync(oldPlaylistName, newPlaylistName);

            if (result == RenamePlaylistResult.Success)
            {
                this.PlaylistsChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> playlists)
        {
            DeletePlaylistResult result = await this.playlistRepository.DeletePlaylistsAsync(playlists);

            if (result == DeletePlaylistResult.Success)
            {
                this.PlaylistsChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            AddPlaylistResult result = await this.playlistRepository.AddPlaylistAsync(playlistName);

            if (result == AddPlaylistResult.Success)
            {
                this.PlaylistsChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            return await this.playlistRepository.GetPlaylistsAsync();
        }

        public async Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName)
        {
            string playlistName = String.Empty;
            var paths = new List<String>();

            // Decode the playlist file
            // ------------------------
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult decodeResult = null;

            await Task.Run(() => decodeResult = decoder.DecodePlaylist(fileName));

            if (!decodeResult.DecodeResult.Result)
            {
                LogClient.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
                return OpenPlaylistResult.Error;
            }

            // Set the paths
            // -------------
            paths = decodeResult.Paths;


            // Get a unique name for the playlist
            // ----------------------------------
            playlistName = await this.playlistRepository.GetUniquePlaylistNameAsync(decodeResult.PlaylistName);

            // Add the Playlist to the database
            // --------------------------------
            AddPlaylistResult addPlaylistResult = await this.playlistRepository.AddPlaylistAsync(playlistName);
            if (addPlaylistResult != AddPlaylistResult.Success) return OpenPlaylistResult.Error;

            // Add Tracks to the Playlist
            // --------------------------
            List<MergedTrack> tracks = await this.trackRepository.GetTracksAsync(paths);
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);
            if (!result.IsSuccess) return OpenPlaylistResult.Error;

            // If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success,
            // so we can always raise the PlaylistsChanged Event.
            this.PlaylistsChanged(this, new EventArgs());

            return OpenPlaylistResult.Success;
        }

        public async Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels)
        {
            List<Album> dbAlbums = await this.albumRepository.GetAlbumsAsync();

            if (albumViewModels != null && albumViewModels.Count > 0)
            {
                await Task.Run(() =>
                {
                    foreach (AlbumViewModel albvm in albumViewModels)
                    {
                        try
                        {
                            // Get an up to date version of this album from the database

                            Album dbAlbum = dbAlbums.Where((a) => a.AlbumID.Equals(albvm.Album.AlbumID)).Select((a) => a).FirstOrDefault();

                            if (dbAlbum != null)
                            {
                                albvm.Album.ArtworkID = dbAlbum.ArtworkID;
                                albvm.ArtworkPath = this.cacheService.GetCachedArtworkPath(dbAlbum.ArtworkID);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Error while refreshing artwork for Album {0}/{1}. Exception: {2}", albvm.AlbumTitle, albvm.AlbumArtist, ex.Message);
                        }
                    }
                });
            }
        }

        public async Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewmodels, int delayMilliSeconds)
        {
            await Task.Delay(delayMilliSeconds);

            await Task.Run(() =>
            {
                try
                {
                    foreach (AlbumViewModel albvm in albumViewmodels)
                    {
                        try
                        {
                            albvm.ArtworkPath = this.cacheService.GetCachedArtworkPath(albvm.Album.ArtworkID);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Error while setting artwork for album with Album artist = '{0}' and Title='{1}'. Exception: {2}", albvm.AlbumArtist, albvm.AlbumTitle, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while setting album artwork. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string fullPlaylistPath, bool generateUniqueName)
        {
            return await this.ExportPlaylistAsync(playlist,
                                         System.IO.Path.GetDirectoryName(fullPlaylistPath),
                                         System.IO.Path.GetFileNameWithoutExtension(fullPlaylistPath), generateUniqueName);
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string destinationDirectory, string playlistName, bool generateUniqueName)
        {
            ExportPlaylistsResult result = ExportPlaylistsResult.Success;

            List<MergedTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(playlist.ToList()), TrackOrder.ByFileName);

            await Task.Run(() =>
            {

                try
                {
                    string playlistFileNameWithoutPathAndExtension = FileUtils.SanitizeFilename(playlistName);
                    string playlistFileFullPath = Path.Combine(destinationDirectory, string.Concat(playlistFileNameWithoutPathAndExtension, FileFormats.M3U));

                    if (generateUniqueName)
                    {
                        // Make sure the file we try to create doesn't exist yet
                        while (System.IO.File.Exists(playlistFileFullPath))
                        {
                            playlistFileNameWithoutPathAndExtension = playlistFileNameWithoutPathAndExtension + " (1)";
                            playlistFileFullPath = Path.Combine(destinationDirectory, string.Concat(playlistFileNameWithoutPathAndExtension, FileFormats.M3U));
                        }
                    }

                    // Write all the paths to the file
                    using (StreamWriter file = new StreamWriter(playlistFileFullPath))
                    {
                        foreach (MergedTrack t in tracks)
                        {
                            string audioFileNameWithoutPath = Path.GetFileName(t.Path);

                            // If the audio file is in the same directory as the playlist file, 
                            // don't save the full path in the playlist file.
                            if (System.IO.File.Exists(Path.Combine(destinationDirectory, audioFileNameWithoutPath)))
                            {
                                file.WriteLine(audioFileNameWithoutPath);
                            }
                            else
                            {
                                file.WriteLine(t.Path);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = ExportPlaylistsResult.Error;
                    LogClient.Error("Error while exporting Playlist '{0}'. Exception: {1}", playlist.PlaylistName, ex.Message);
                }
            });

            return result;
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistsAsync(IList<Playlist> playlists, string destinationDirectory)
        {
            ExportPlaylistsResult result = ExportPlaylistsResult.Success;

            foreach (Playlist pl in playlists)
            {
                ExportPlaylistsResult tempResult = await this.ExportPlaylistAsync(pl, destinationDirectory, pl.PlaylistName, true);

                // If at least 1 export failed, return an error
                if (tempResult == ExportPlaylistsResult.Error)
                {
                    result = tempResult;
                }
            }

            return result;
        }

        public async Task MarkFolderAsync(Folder fol)
        {
            await Task.Run(() =>
            {
                try
                {
                    lock (this.markedFolders)
                    {
                        if (this.markedFolders.Contains(fol))
                        {
                            this.markedFolders[this.markedFolders.IndexOf(fol)].ShowInCollection = fol.ShowInCollection;
                        }
                        else
                        {
                            this.markedFolders.Add(fol);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error marking folder with path='{0}'. Exception: {1}", fol.Path, ex.Message);
                }
            });
        }

        public async Task SaveMarkedFoldersAsync()
        {
            bool isCollectionChanged = false;

            try
            {
                isCollectionChanged = this.markedFolders.Count > 0;
                await this.folderRepository.UpdateFoldersAsync(this.markedFolders);
                this.markedFolders.Clear();
            }
            catch (Exception ex)
            {
                LogClient.Error("Error updating folders. Exception: {0}", ex.Message);
            }

            if (isCollectionChanged) this.CollectionChanged(this, new EventArgs());
        }
        #endregion
    }
}
