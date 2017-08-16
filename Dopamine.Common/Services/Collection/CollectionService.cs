using Dopamine.Core.Logging;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        #region Variables
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
        public CollectionService(IAlbumRepository albumRepository, IArtistRepository artistRepository, ITrackRepository trackRepository, IGenreRepository genreRepository, IFolderRepository folderRepository, ICacheService cacheService, IPlaybackService playbackService)
        {
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
        #endregion

        #region ICollectionService
        public async Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks)
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

        public async Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks)
        {
            var sendToRecycleBinResult = RemoveTracksResult.Success;
            var result = await this.trackRepository.RemoveTracksAsync(selectedTracks);

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

                    try
                    {
                        // Delete file from disk
                        FileUtils.SendToRecycleBinSilent(track.Path);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error($"Error while removing track '{track.TrackTitle}' from disk. Exception: {ex.Message}");
                        sendToRecycleBinResult = RemoveTracksResult.Error;
                    }
                }

                this.CollectionChanged(this, new EventArgs());
            }

            if (sendToRecycleBinResult == RemoveTracksResult.Success && result == RemoveTracksResult.Success)
                return RemoveTracksResult.Success;
            return RemoveTracksResult.Error;
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
