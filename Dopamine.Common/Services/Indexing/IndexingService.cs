using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Settings;
using Digimezzo.Utilities.Log;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Indexing
{
    public class IndexingService : IIndexingService
    {
        #region Variables
        // Services
        private ICacheService cacheService;

        // Settings
        private ISettings settings;

        // Repositories
        private ITrackRepository trackRepository;
        private IFolderRepository folderRepository;
        private IAlbumRepository albumRepository;
        private IArtistRepository artistRepository;
        private IGenreRepository genreRepository;

        // Watcher
        private FolderWatcherManager watcherManager;

        // Paths
        private List<Tuple<long, string, long>> allDiskPaths;
        private List<Tuple<long, string, long>> newDiskPaths;

        // Cache
        private IndexerCache cache;

        // Factory
        private ISQLiteConnectionFactory factory;

        // IndexingEventArgs
        private IndexingStatusEventArgs eventArgs;

        // Flags
        private bool needsCollectionCheck;
        private bool isIndexing;
        #endregion

        #region Properties
        public bool IsIndexing
        {
            get { return this.isIndexing; }
        }
        #endregion

        #region Construction
        public IndexingService(ISQLiteConnectionFactory factory, ICacheService cacheService, ITrackRepository trackRepository,
            IAlbumRepository albumRepository, IGenreRepository genreRepository, IArtistRepository artistRepository,
            IFolderRepository folderRepository, ISettings settings)
        {
            this.cacheService = cacheService;
            this.factory = factory;
            this.trackRepository = trackRepository;
            this.albumRepository = albumRepository;
            this.genreRepository = genreRepository;
            this.artistRepository = artistRepository;
            this.folderRepository = folderRepository;
            this.settings = settings;

            this.watcherManager = new FolderWatcherManager(this.folderRepository);

            this.settings.RefreshCollectionAutomaticallyChanged += Settings_RefreshCollectionAutomaticallyChanged;
            this.watcherManager.FoldersChanged += WatcherManager_FoldersChanged;

            this.needsCollectionCheck = this.settings.RefreshCollectionAutomatically;
            this.isIndexing = false;
        }
        #endregion

        #region IIndexingService
        public async void OnFoldersChanged()
        {
            if (this.settings.RefreshCollectionAutomatically)
            {
                this.needsCollectionCheck = true;
                await this.watcherManager.StartWatchingAsync();
            }
        }

        public async Task CheckCollectionAsync()
        {
            if (!this.settings.RefreshCollectionAutomatically)
            {
                return;
            }

            if (this.IsIndexing)
            {
                return;
            }

            if (!this.needsCollectionCheck)
            {
                return;
            }

            this.needsCollectionCheck = false;

            await this.watcherManager.StopWatchingAsync();
            await this.InitializeAsync();

            try
            {
                using (var conn = this.factory.GetConnection())
                {
                    long databaseNeedsIndexingCount = conn.Table<Track>().Select(t => t).ToList().Where(t => t.NeedsIndexing == 1).LongCount();
                    long databaseLastDateFileModified = conn.Table<Track>().Select(t => t).ToList().OrderByDescending(t => t.DateFileModified).Select(t => t.DateFileModified).FirstOrDefault();
                    long diskLastDateFileModified = this.allDiskPaths.Count > 0 ? this.allDiskPaths.Select((t) => t.Item3).OrderByDescending((t) => t).First() : 0;
                    long databaseTrackCount = conn.Table<Track>().Select(t => t).LongCount();

                    if (databaseNeedsIndexingCount > 0 |
                        databaseTrackCount != this.allDiskPaths.Count |
                        databaseLastDateFileModified < diskLastDateFileModified)
                    {
                        await Task.Delay(1000);
                        await this.IndexCollectionAsync(true);
                    }
                    else
                    {
                        if (this.settings.RefreshCollectionAutomatically)
                        {
                            await this.watcherManager.StartWatchingAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get indexing statistics from database. Exception: {0}", ex.Message);
            }
        }

        public async Task IndexCollectionAsync(bool isCheckPerformed = false)
        {
            if (this.IsIndexing)
            {
                return;
            }

            await this.watcherManager.StopWatchingAsync();

            this.isIndexing = true;
            this.IndexingStarted(this, new EventArgs());

            if (!isCheckPerformed)
            {
                // CheckCollectionAsync() already performs InitializeAsync(). Make sure we don't
                // execute this a second time if we're started from CheckCollectionAsync().
                await this.InitializeAsync();
            }

            // Tracks
            // ------

            bool isTracksChanged = await this.IndexTracksAsync(this.settings.IgnoreRemovedFiles) > 0 ? true : false;

            if (isTracksChanged)
            {
                LogClient.Info("Sending event to refresh the lists");
                this.RefreshLists(this, new EventArgs());
            }

            // Artwork
            // -------
            bool isArtworkChanged = await this.IndexArtworkAsync() > 0 ? true : false;

            if (isArtworkChanged)
            {
                LogClient.Info("Sending event to refresh the artwork");
                this.RefreshArtwork(this, new EventArgs());
            }

            // Finalize
            // --------
            this.isIndexing = false;
            this.IndexingStopped(this, new EventArgs());

            if (this.settings.RefreshCollectionAutomatically)
            {
                await this.watcherManager.StartWatchingAsync();
            }
        }
        #endregion

        #region Private
        private async Task InitializeAsync()
        {
            // Initialize Cache
            this.cache = new IndexerCache(this.factory);

            // IndexingEventArgs
            this.eventArgs = new IndexingStatusEventArgs();
            this.eventArgs.IndexingAction = IndexingAction.Idle;

            // Get all files on disk which belong to a Collection Folder
            this.allDiskPaths = await this.folderRepository.GetPathsAsync();
        }

        private async Task<long> IndexArtworkAsync()
        {
            LogClient.Info("+++ STARTED INDEXING ARTWORK +++");

            DateTime startTime = DateTime.Now;

            long numberDeletedFromDatabase = 0;
            long numberDeletedFromDisk = 0;
            long numberUpdated = 0;

            try
            {
                // Step 1: delete unused artwork from the database
                // -----------------------------------------------
                numberDeletedFromDatabase = await this.DeleteUnusedArtworkFromDatabaseAsync();

                // Step 2: add new artwork to the cache
                // -------------------------------------
                numberUpdated = await this.AddArtworkAsync();

                // Step 3: delete unused artwork from the cache
                // --------------------------------------------
                numberDeletedFromDisk = await this.DeleteUnusedArtworkFromCacheAsync();
            }
            catch (Exception ex)
            {
                LogClient.Info("There was a problem while updating the artwork. Exception: {0}", ex.Message);
            }

            LogClient.Info("+++ FINISHED INDEXING ARTWORK: Covers deleted from database: {0}. Covers deleted from disk: {1}. Covers updated: {2}. Time required: {3} ms +++", numberDeletedFromDatabase, numberDeletedFromDisk, numberUpdated, Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));

            return numberDeletedFromDatabase + numberDeletedFromDisk + numberUpdated;
        }

        private async Task<long> AddArtworkAsync()
        {
            long numberUpdated = 0;

            await Task.Run(async () =>
            {
                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    conn.BeginTransaction();

                    foreach (Album alb in conn.Table<Album>())
                    {
                        try
                        {
                            Track trk = this.GetLastModifiedTrack(alb);

                            alb.ArtworkID = await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(alb, trk.Path));

                            if (!string.IsNullOrEmpty(alb.ArtworkID))
                            {
                                alb.DateLastSynced = DateTime.Now.Ticks;
                                conn.Update(alb);
                                numberUpdated += 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("There was a problem while updating the cover art for Album {0}/{1}. Exception: {2}", alb.AlbumTitle, alb.AlbumArtist, ex.Message);
                        }

                        // Report progress if at least 1 album is added
                        if (numberUpdated > 0)
                        {
                            this.eventArgs.IndexingAction = IndexingAction.UpdateArtwork;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);
                        }
                    }

                    conn.Commit();
                }
            });

            return numberUpdated;
        }

        private async Task<long> DeleteUnusedArtworkFromDatabaseAsync()
        {
            long numberDeleted = 0;

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    conn.BeginTransaction();

                    foreach (Album alb in conn.Table<Album>().Where((a) => (a.ArtworkID != null && a.ArtworkID != string.Empty)))
                    {
                        if (!System.IO.File.Exists(this.cacheService.GetCachedArtworkPath(alb.ArtworkID)))
                        {
                            alb.ArtworkID = string.Empty;
                            conn.Update(alb);
                            numberDeleted += 1;
                        }

                        // Report progress if at least 1 cover is deleted
                        if (numberDeleted > 0)
                        {
                            this.eventArgs.IndexingAction = IndexingAction.UpdateArtwork;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);
                        }
                    }

                    conn.Commit();
                }
            });

            return numberDeleted;
        }

        private async Task<long> DeleteUnusedArtworkFromCacheAsync()
        {
            long numberDeleted = 0;

            await Task.Run(() =>
            {
                string[] artworkFiles = Directory.GetFiles(this.cacheService.CoverArtCacheFolderPath, "album-*.jpg");

                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    List<Album> albumsWithArtwork = conn.Table<Album>().Where((t) => t.ArtworkID != null && t.ArtworkID != string.Empty).Select((t) => t).ToList();
                    List<string> artworkIDs = albumsWithArtwork.Select((a) => a.ArtworkID).ToList();

                    foreach (string artworkFile in artworkFiles)
                    {
                        if (!artworkIDs.Contains(System.IO.Path.GetFileNameWithoutExtension(artworkFile)))
                        {
                            try
                            {
                                System.IO.File.Delete(artworkFile);
                                numberDeleted += 1;
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while deleting cached artwork {0}. Exception: {1}", artworkFile, ex.Message);
                            }
                        }

                        // Report progress if at least 1 cover is deleted
                        if (numberDeleted > 0)
                        {
                            this.eventArgs.IndexingAction = IndexingAction.UpdateArtwork;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);
                        }
                    }
                }
            });

            return numberDeleted;
        }

        private Track GetLastModifiedTrack(Album album)
        {
            // Get the Track from this Album which was last modified
            Track lastModifiedTrack = null;

            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                lastModifiedTrack = conn.Table<Track>().Where((t) => t.AlbumID.Equals(album.AlbumID)).Select((t) => t).OrderByDescending((t) => t.DateFileModified).FirstOrDefault();
            }

            return lastModifiedTrack;
        }

        private async Task<long> IndexTracksAsync(bool ignoreRemovedFiles)
        {
            LogClient.Info("+++ STARTED INDEXING COLLECTION +++");

            DateTime startTime = DateTime.Now;

            long numberTracksRemoved = 0;
            long numberTracksAdded = 0;
            long numberTracksUpdated = 0;

            try
            {
                // Step 1: remove Tracks which are not found on disk
                // -------------------------------------------------
                DateTime removeTracksStartTime = DateTime.Now;

                numberTracksRemoved = await this.RemoveTracksAsync();

                LogClient.Info("Tracks removed: {0}. Time required: {1} ms +++", numberTracksRemoved, Convert.ToInt64(DateTime.Now.Subtract(removeTracksStartTime).TotalMilliseconds));

                await this.GetNewDiskPathsAsync(ignoreRemovedFiles); // Obsolete Tracks are removed, now we can determine new files

                // Step 2: update outdated Tracks
                // ------------------------------
                DateTime updateTracksStartTime = DateTime.Now;
                numberTracksUpdated = await this.UpdateTracksAsync();

                LogClient.Info("Tracks updated: {0}. Time required: {1} ms +++", numberTracksUpdated, Convert.ToInt64(DateTime.Now.Subtract(updateTracksStartTime).TotalMilliseconds));

                // Step 3: add new Tracks
                // ----------------------
                DateTime addTracksStartTime = DateTime.Now;
                numberTracksAdded = await this.AddTracksAsync();

                LogClient.Info("Tracks added: {0}. Time required: {1} ms +++", numberTracksAdded, Convert.ToInt64(DateTime.Now.Subtract(addTracksStartTime).TotalMilliseconds));

                // Step 4: delete orphans
                // ----------------------
                await this.albumRepository.DeleteOrphanedAlbumsAsync(); // Delete orphaned Albums
                await this.artistRepository.DeleteOrphanedArtistsAsync(); // Delete orphaned Artists
                await this.genreRepository.DeleteOrphanedGenresAsync(); // Delete orphaned Genres
            }
            catch (Exception ex)
            {
                LogClient.Info("There was a problem while indexing the collection. Exception: {0}", ex.Message);
            }

            LogClient.Info("+++ FINISHED INDEXING COLLECTION: Tracks removed: {0}. Tracks updated: {1}. Tracks added: {2}. Time required: {3} ms +++", numberTracksRemoved, numberTracksUpdated, numberTracksAdded, Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));

            return numberTracksRemoved + numberTracksAdded + numberTracksUpdated;
        }

        private async Task GetNewDiskPathsAsync(bool ignoreRemovedFiles)
        {
            await Task.Run(() =>
            {
                var dbPaths = new List<string>();

                using (var conn = this.factory.GetConnection())
                {
                    dbPaths = conn.Table<Track>().ToList().Select((trk) => trk.SafePath).ToList();
                }

                var removedPaths = new List<string>();

                using (var conn = this.factory.GetConnection())
                {
                    removedPaths = conn.Table<RemovedTrack>().ToList().Select((t) => t.SafePath).ToList();
                }

                this.newDiskPaths = new List<Tuple<long, string, long>>();

                foreach (Tuple<long, string, long> diskpath in this.allDiskPaths)
                {
                    if (!dbPaths.Contains(diskpath.Item2.ToLower()) && (ignoreRemovedFiles ? !removedPaths.Contains(diskpath.Item2.ToLower()) : true))
                    {
                        this.newDiskPaths.Add(diskpath);
                    }
                }
            });
        }

        private async Task<long> RemoveTracksAsync()
        {
            long numberRemovedTracks = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        // Create a list of folderIDs
                        List<long> folderIDs = conn.Table<Folder>().ToList().Select((t) => t.FolderID).ToList();

                        List<Track> alltracks = conn.Table<Track>().Select((t) => t).ToList();
                        List<Track> tracksInMissingFolders = alltracks.Select((t) => t).Where(t => !folderIDs.Contains(t.FolderID)).ToList();
                        List<Track> remainingTracks = new List<Track>();

                        // Processing tracks in missing folders in bulk first, then checking 
                        // existence of the remaining tracks, improves speed of removing tracks.
                        if (tracksInMissingFolders.Count > 0 && tracksInMissingFolders.Count < alltracks.Count)
                        {
                            remainingTracks = alltracks.Except(tracksInMissingFolders).ToList();
                        }
                        else
                        {
                            remainingTracks = alltracks;
                        }

                        // 1. Process tracks in missing folders
                        // ------------------------------------
                        if (tracksInMissingFolders.Count > 0)
                        {
                            // Report progress immediately, as there are tracks in missing folders.
                            this.eventArgs.IndexingAction = IndexingAction.RemoveTracks;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);

                            // Delete
                            foreach (Track trk in tracksInMissingFolders)
                            {
                                conn.Delete(trk);
                            }

                            numberRemovedTracks += tracksInMissingFolders.Count;
                        }

                        // 2. Process remaining tracks
                        // ---------------------------
                        if (remainingTracks.Count > 0)
                        {
                            foreach (Track trk in remainingTracks)
                            {
                                // If a remaining track doesn't exist on disk, delete it from the collection.
                                if (!System.IO.File.Exists(trk.Path))
                                {
                                    conn.Delete(trk);
                                    numberRemovedTracks += 1;
                                }

                                // Report progress if at least 1 track is removed
                                if (numberRemovedTracks > 0)
                                {
                                    this.eventArgs.IndexingAction = IndexingAction.RemoveTracks;
                                    this.eventArgs.ProgressPercent = 0;
                                    this.IndexingStatusChanged(this.eventArgs);
                                }
                            }
                        }

                        conn.Commit();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("There was a problem while removing Tracks. Exception: {0}", ex.Message);
                }
            });

            return numberRemovedTracks;
        }

        private async Task<long> UpdateTracksAsync()
        {
            long numberUpdatedTracks = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        List<Track> alltracks = conn.Table<Track>().Select((t) => t).ToList();

                        long currentValue = 0;
                        long totalValue = alltracks.Count;

                        foreach (Track dbTrack in alltracks)
                        {
                            try
                            {
                                if (IndexerUtils.IsTrackOutdated(dbTrack) | dbTrack.NeedsIndexing == 1)
                                {
                                    if (this.ProcessTrack(dbTrack, conn))
                                    {
                                        conn.Update(dbTrack);
                                        numberUpdatedTracks += 1;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while updating Track with path='{0}'. Exception: {1}", dbTrack.Path, ex.Message);
                            }

                            currentValue += 1;

                            // Report progress if at least 1 track is updated
                            if (numberUpdatedTracks > 0)
                            {
                                this.eventArgs.IndexingAction = IndexingAction.UpdateTracks;
                                this.eventArgs.ProgressCurrent = currentValue;
                                this.eventArgs.ProgressTotal = totalValue;
                                this.eventArgs.ProgressPercent = IndexerUtils.CalculatePercent(currentValue, totalValue);
                                this.IndexingStatusChanged(this.eventArgs);
                            }
                        }

                        conn.Commit();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("There was a problem while updating Tracks. Exception: {0}", ex.Message);
                }
            });

            return numberUpdatedTracks;
        }

        private async Task<long> AddTracksAsync()
        {
            long numberAddedTracks = 0;

            await Task.Run(() =>
            {
                try
                {
                    long currentValue = 0;
                    long totalValue = this.newDiskPaths.Count;

                    long saveItemCount = IndexerUtils.CalculateSaveItemCount(this.newDiskPaths.Count);
                    long unsavedItemCount = 0;

                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        foreach (Tuple<long, string, long> newDiskPath in this.newDiskPaths)
                        {
                            Track diskTrack = new Track
                            {
                                FolderID = newDiskPath.Item1,
                                Path = newDiskPath.Item2,
                                DateAdded = DateTime.Now.Ticks
                            };

                            try
                            {
                                if (this.ProcessTrack(diskTrack, conn))
                                {
                                    conn.Insert(diskTrack);
                                    numberAddedTracks += 1;
                                    unsavedItemCount += 1;
                                }

                                // Intermediate save to the database if 20% is reached
                                if (unsavedItemCount == saveItemCount)
                                {
                                    unsavedItemCount = 0;
                                    conn.Commit(); // Intermediate save
                                    conn.BeginTransaction();
                                }

                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while updating Track with path='{0}'. Exception: {1}", diskTrack.Path, ex.Message);
                            }

                            currentValue += 1;

                            // Report progress if at least 1 track is updated
                            if (numberAddedTracks > 0)
                            {
                                this.eventArgs.IndexingAction = IndexingAction.AddTracks;
                                this.eventArgs.ProgressCurrent = currentValue;
                                this.eventArgs.ProgressTotal = totalValue;
                                this.eventArgs.ProgressPercent = IndexerUtils.CalculatePercent(currentValue, totalValue);
                                this.IndexingStatusChanged(this.eventArgs);
                            }
                        }

                        conn.Commit(); // Final save
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("There was a problem while adding Tracks. Exception: {0}", ex.Message);
                }
            });

            return numberAddedTracks;
        }

        private bool ProcessTrack(Track track, SQLiteConnection conn)
        {
            bool processingSuccessful = false;

            var newTrackStatistic = new TrackStatistic();
            var newAlbum = new Album();
            var newArtist = new Artist();
            var newGenre = new Genre();

            try
            {
                MetadataUtils.SplitMetadata(track.Path, ref track, ref newTrackStatistic, ref newAlbum, ref newArtist, ref newGenre);
                processingSuccessful = true;
            }
            catch (Exception ex)
            {
                processingSuccessful = false;
                LogClient.Error("Error while retrieving tag information for file {0}. File not added to the database. Exception: {1}", track.Path, ex.Message);
            }

            if (processingSuccessful)
            {
                // Check if such TrackStatistic already exists in the database
                if (!this.cache.HasCachedTrackStatistic(newTrackStatistic))
                {
                    // If not, add it.
                    conn.Insert(newTrackStatistic);
                }

                // Check if such Artist already exists in the database
                if (!this.cache.HasCachedArtist(ref newArtist))
                {
                    // If not, add it.
                    conn.Insert(newArtist);
                }

                // Check if such Genre already exists in the database 
                if (!this.cache.HasCachedGenre(ref newGenre))
                {
                    // If not, add it.
                    conn.Insert(newGenre);
                }

                // Check if such Album already exists in the database
                if (!this.cache.HasCachedAlbum(ref newAlbum))
                {
                    // If Not, add it.
                    conn.Insert(newAlbum);
                }
                else
                {
                    // Make sure the Year of the existing album is updated
                    Album dbAlbum = conn.Table<Album>().Where((a) => a.AlbumID.Equals(newAlbum.AlbumID)).FirstOrDefault();

                    if (dbAlbum != null)
                    {
                        dbAlbum.Year = newAlbum.Year;
                        conn.Update(dbAlbum);
                    }
                }

                track.AlbumID = newAlbum.AlbumID;
                track.ArtistID = newArtist.ArtistID;
                track.GenreID = newGenre.GenreID;
            }

            return processingSuccessful;
        }

        private async void Settings_RefreshCollectionAutomaticallyChanged(bool refreshCollectionAutomatically)
        {
            this.needsCollectionCheck = refreshCollectionAutomatically;

            if (refreshCollectionAutomatically)
            {
                await this.watcherManager.StartWatchingAsync();
            }
            else
            {
                await this.watcherManager.StopWatchingAsync();
            }
        }

        private async void WatcherManager_FoldersChanged(object sender, EventArgs e)
        {
            await this.IndexCollectionAsync();
        }
        #endregion

        #region Events
        public event EventHandler IndexingStopped = delegate { };
        public event EventHandler IndexingStarted = delegate { };
        public event Action<IndexingStatusEventArgs> IndexingStatusChanged = delegate { };
        public event EventHandler RefreshLists = delegate { };
        public event EventHandler RefreshArtwork = delegate { };
        #endregion
    }
}
