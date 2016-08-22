using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Helpers;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private List<Folder> markedFolders;
        #endregion

        #region Construction
        public CollectionService(IPlaylistRepository playlistRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, ITrackRepository trackRepository, IGenreRepository genreRepository, IFolderRepository folderRepository)
        {
            this.playlistRepository = playlistRepository;
            this.albumRepository = albumRepository;
            this.artistRepository = artistRepository;
            this.trackRepository = trackRepository;
            this.genreRepository = genreRepository;
            this.folderRepository = folderRepository;
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
            AddToPlaylistResult result = await this.playlistRepository.AddArtistsToPlaylistAsync(artists, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            AddToPlaylistResult result = await this.playlistRepository.AddGenresToPlaylistAsync(genres, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackInfo> tracks, string playlistName)
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
            AddToPlaylistResult result = await this.playlistRepository.AddAlbumsToPlaylistAsync(albums, playlistName);

            if (result.IsSuccess)
            {
                this.AddedTracksToPlaylist(result.NumberTracksAdded, playlistName);
            }

            return result;
        }

        public async Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<TrackInfo> tracks, Playlist selectedPlaylist)
        {
            DeleteTracksFromPlaylistsResult result = await this.playlistRepository.DeleteTracksFromPlaylistAsync(tracks, selectedPlaylist);

            if (result == DeleteTracksFromPlaylistsResult.Success)
            {
                this.DeletedTracksFromPlaylists(this, new EventArgs());
            }

            return result;
        }

        public async Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<TrackInfo> selectedTracks)
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
                LogClient.Instance.Logger.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
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

            // Add TrackInfo's to the Playlist
            // -------------------------------
            List<TrackInfo> tracks = await this.trackRepository.GetTracksAsync(paths);
            AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlistName);
            if (!result.IsSuccess) return OpenPlaylistResult.Error;

            // If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success,
            // so we can always raise the PlaylistsChanged Event.
            this.PlaylistsChanged(this, new EventArgs());

            return OpenPlaylistResult.Success;
        }

        public async Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, ObservableCollection<TrackInfoViewModel> trackInfoViewModels)
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
                                albvm.ArtworkPath = ArtworkUtils.GetArtworkPath(dbAlbum);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Error while refreshing artwork for Album {0}/{1}. Exception: {2}", albvm.AlbumTitle, albvm.AlbumArtist, ex.Message);
                        }
                    }
                });
            }

            if (trackInfoViewModels != null && trackInfoViewModels.Count > 0)
            {
                await Task.Run(() =>
                {
                    foreach (TrackInfoViewModel tivm in trackInfoViewModels)
                    {
                        try
                        {
                            // Get an up to date version of this album from the database
                            Album dbAlbum = dbAlbums.Where((a) => a.AlbumID.Equals(tivm.TrackInfo.AlbumID)).Select((a) => a).FirstOrDefault();

                            if (dbAlbum != null)
                            {
                                tivm.TrackInfo.AlbumArtworkID = dbAlbum.ArtworkID;
                                tivm.ArtworkPath = ArtworkUtils.GetArtworkPath(dbAlbum);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Error while refreshing artwork for TrackInfo with path {0}. Exception: {1}", tivm.TrackInfo.Path, ex.Message);
                        }
                    }
                });
            }
        }

        public async Task SetTrackArtworkAsync(ObservableCollection<TrackInfoViewModel> trackInfoViewModels, int delayMilliSeconds)
        {
            await Task.Delay(delayMilliSeconds);

            await Task.Run(() =>
            {
                try
                {
                    foreach (TrackInfoViewModel tivm in trackInfoViewModels)
                    {
                        try
                        {
                            tivm.ArtworkPath = ArtworkUtils.GetArtworkPath(tivm.TrackInfo.Album);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Error while setting artwork for Track {0}. Exception: {1}", tivm.TrackInfo.Path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Error while setting track artwork. Exception: {0}", ex.Message);
                }
            });
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
                            albvm.ArtworkPath = ArtworkUtils.GetArtworkPath(albvm.Album);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Error while setting artwork for album with Album artist = '{0}' and Title='{1}'. Exception: {2}", albvm.AlbumArtist, albvm.AlbumTitle, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Error while setting album artwork. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string fullPlaylistPath, bool generateUniqueName)
        {
            return await this.ExportPlaylistAsync(playlist,
                                         System.IO.Path.GetDirectoryName(fullPlaylistPath),
                                         System.IO.Path.GetFileNameWithoutExtension(fullPlaylistPath), generateUniqueName);
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistAsync(Playlist playlist, string destinationDirectory, string iPlaylistName, bool generateUniqueName)
        {
            ExportPlaylistsResult result = ExportPlaylistsResult.Success;

            List<TrackInfo> tracks = await Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(playlist.ToList()), TrackOrder.ByFileName);

            await Task.Run(() => {

                try
                {
                    string playlistFileNameWithoutPathAndExtension = FileOperations.SanitizeFilename(iPlaylistName);
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
                        foreach (TrackInfo t in tracks)
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
                    LogClient.Instance.Logger.Error("Error while exporting Playlist '{0}'. Exception: {1}", playlist, ex.Message);
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
                if( tempResult == ExportPlaylistsResult.Error)
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
                    LogClient.Instance.Logger.Error("Error marking folder with path='{0}'. Exception: {1}", fol.Path, ex.Message);
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
                LogClient.Instance.Logger.Error("Error updating folders. Exception: {0}", ex.Message);
            }

            if (isCollectionChanged) this.CollectionChanged(this, new EventArgs());
        }
        #endregion
    }
}
