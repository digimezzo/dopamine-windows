using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Dopamine.Common.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        private IAlbumRepository albumRepository;
        private IArtistRepository artistRepository;
        private ITrackRepository trackRepository;
        private IGenreRepository genreRepository;
        private IFolderRepository folderRepository;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private List<Folder> markedFolders;
        private Timer saveMarkedFoldersTimer = new Timer(2000);
    
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

            this.saveMarkedFoldersTimer.Elapsed += SaveMarkedFoldersTimer_Elapsed;
        }
   
        public event EventHandler CollectionChanged = delegate { };
  
        private async Task SaveMarkedFoldersAsync()
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

            if (isCollectionChanged)
            {
                // Execute on Dispatcher as this will cause a refresh of the lists
                Application.Current.Dispatcher.Invoke(() => this.CollectionChanged(this, new EventArgs()));
            }
        }

        private async void SaveMarkedFoldersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await this.SaveMarkedFoldersAsync();
        }

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

        public async Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, List<long> albumsIds = null)
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
                            if(albumsIds == null || albumsIds.Contains(albvm.Album.AlbumID))
                            {
                                // Get an up to date version of this album from the database
                                Album dbAlbum = dbAlbums.Where((a) => a.AlbumID.Equals(albvm.Album.AlbumID)).Select((a) => a).FirstOrDefault();

                                if (dbAlbum != null)
                                {
                                    albvm.Album.ArtworkID = dbAlbum.ArtworkID;
                                    albvm.ArtworkPath = this.cacheService.GetCachedArtworkPath(dbAlbum.ArtworkID);
                                }
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

        public async Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, int delayMilliSeconds)
        {
            await Task.Delay(delayMilliSeconds);

            await Task.Run(() =>
            {
                try
                {
                    foreach (AlbumViewModel albvm in albumViewModels)
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
            this.saveMarkedFoldersTimer.Stop();

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

                    this.saveMarkedFoldersTimer.Start();
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error marking folder with path='{0}'. Exception: {1}", fol.Path, ex.Message);
                }
            });
        }
    }
}
