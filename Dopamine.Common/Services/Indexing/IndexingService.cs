using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Cache;
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
        // Services
        private ICacheService cacheService;

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
        private bool isIndexing;
        private bool isFoldersChanged;

        public bool IsIndexing
        {
            get { return this.isIndexing; }
        }

        public IndexingService(ISQLiteConnectionFactory factory, ICacheService cacheService, ITrackRepository trackRepository,
            IAlbumRepository albumRepository, IGenreRepository genreRepository, IArtistRepository artistRepository,
            IFolderRepository folderRepository)
        {
            this.cacheService = cacheService;
            this.factory = factory;
            this.trackRepository = trackRepository;
            this.albumRepository = albumRepository;
            this.genreRepository = genreRepository;
            this.artistRepository = artistRepository;
            this.folderRepository = folderRepository;

            this.watcherManager = new FolderWatcherManager(this.folderRepository);

            SettingsClient.SettingChanged += SettingsClient_SettingChanged;
            this.watcherManager.FoldersChanged += WatcherManager_FoldersChanged;

            this.isIndexing = false;
        }

        private async void SettingsClient_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (SettingsClient.IsSettingChanged(e, "Indexing", "RefreshCollectionAutomatically"))
            {
                if ((bool)e.SettingValue)
                {
                    await this.watcherManager.StartWatchingAsync();
                }
                else
                {
                    await this.watcherManager.StopWatchingAsync();
                }
            }
        }

        public async void OnFoldersChanged()
        {
            this.isFoldersChanged = true;

            if (SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically"))
            {
                await this.watcherManager.StartWatchingAsync();
            }
        }

        public async Task RefreshCollectionAsync()
        {
            if (!SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically"))
            {
                return;
            }

            await this.CheckCollectionAsync(false);
        }

        public async Task RefreshCollectionIfFoldersChangedAsync()
        {
            if (!this.isFoldersChanged)
            {
                return;
            }

            await this.RefreshCollectionAsync();
        }

        public async Task RefreshCollectionImmediatelyAsync()
        {
            await this.CheckCollectionAsync(true);
        }

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

        private async Task CheckCollectionAsync(bool forceIndexing)
        {
            if (this.IsIndexing)
            {
                return;
            }

            await this.watcherManager.StopWatchingAsync();
            await this.InitializeAsync();

            try
            {
                using (var conn = this.factory.GetConnection())
                {
                    bool performIndexing = false;

                    if (forceIndexing)
                    {
                        performIndexing = true;
                    }
                    else
                    {
                        long databaseNeedsIndexingCount = conn.Table<Track>().Select(t => t).ToList().Where(t => t.NeedsIndexing == 1).LongCount();
                        long databaseLastDateFileModified = conn.Table<Track>().Select(t => t).ToList().OrderByDescending(t => t.DateFileModified).Select(t => t.DateFileModified).FirstOrDefault();
                        long diskLastDateFileModified = this.allDiskPaths.Count > 0 ? this.allDiskPaths.Select((t) => t.Item3).OrderByDescending((t) => t).First() : 0;
                        long databaseTrackCount = conn.Table<Track>().Select(t => t).LongCount();

                        performIndexing = databaseNeedsIndexingCount > 0 |
                                          databaseTrackCount != this.allDiskPaths.Count |
                                          databaseLastDateFileModified < diskLastDateFileModified;
                    }

                    if (performIndexing)
                    {
                        await Task.Delay(1000);
                        await this.IndexCollectionAsync();
                    }
                    else
                    {
                        if (SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically"))
                        {
                            await this.watcherManager.StartWatchingAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not check the collection. Exception: {0}", ex.Message);
            }
        }

        private async Task IndexCollectionAsync()
        {
            if (this.IsIndexing)
            {
                return;
            }

            this.isIndexing = true;
            this.IndexingStarted(this, new EventArgs());

            // Tracks
            // ------
            bool isTracksChanged = await this.IndexTracksAsync(SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles")) > 0 ? true : false;

            if (isTracksChanged)
            {
                LogClient.Info("Sending event to refresh the lists");
                this.RefreshLists(this, new EventArgs());
            }

            // Finalize
            // --------
            this.isIndexing = false;
            this.IndexingStopped(this, new EventArgs());

            if (SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically"))
            {
                await this.watcherManager.StartWatchingAsync();
            }
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

                                    // Report progress as soon as the first track was removed.
                                    // This is indeterminate progress. No need to sent it multiple times.
                                    if (numberRemovedTracks == 1)
                                    {
                                        this.eventArgs.IndexingAction = IndexingAction.RemoveTracks;
                                        this.eventArgs.ProgressPercent = 0;
                                        this.IndexingStatusChanged(this.eventArgs);
                                    }
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

                        long progressInterval = IndexerUtils.CalculateProgessInterval(totalValue);
                        long lastProgressValue = 0;

                        foreach (Track dbTrack in alltracks)
                        {
                            try
                            {
                                if (IndexerUtils.IsTrackOutdated(dbTrack) | dbTrack.NeedsIndexing == 1)
                                {
                                    this.ProcessTrack(dbTrack, conn);
                                    conn.Update(dbTrack);
                                    numberUpdatedTracks += 1;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while updating Track with path='{0}'. Exception: {1}", dbTrack.Path, ex.Message);
                            }

                            currentValue += 1;

                            // Report progress if at least 1 track is updated OR when the progress
                            // interval has been exceeded OR the maximum has been reached.
                            bool mustReportProgress = numberUpdatedTracks == 1 ||
                            currentValue - lastProgressValue > progressInterval ||
                            currentValue == totalValue;

                            if (mustReportProgress)
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

                    long saveItemCount = IndexerUtils.CalculateSaveItemCount(totalValue);
                    long unsavedItemCount = 0;

                    long progressInterval = IndexerUtils.CalculateProgessInterval(totalValue);
                    long lastProgressValue = 0;

                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        foreach (Tuple<long, string, long> newDiskPath in this.newDiskPaths)
                        {
                            Track diskTrack = Track.CreateDefault(newDiskPath.Item1, newDiskPath.Item2);

                            try
                            {
                                this.ProcessTrack(diskTrack, conn);
                                conn.Insert(diskTrack);
                                numberAddedTracks += 1;
                                unsavedItemCount += 1;

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

                            // Report progress if at least 1 track is added OR when the progress
                            // interval has been exceeded OR the maximum has been reached.
                            bool mustReportProgress = numberAddedTracks == 1 ||
                            currentValue - lastProgressValue > progressInterval ||
                            currentValue == totalValue;

                            if (mustReportProgress)
                            {
                                lastProgressValue = currentValue;

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

        private void ProcessTrack(Track track, SQLiteConnection conn)
        {
            var newTrackStatistic = new TrackStatistic();
            var newAlbum = new Album();
            var newArtist = new Artist();
            var newGenre = new Genre();

            try
            {
                MetadataUtils.SplitMetadata(track.Path, ref track, ref newTrackStatistic, ref newAlbum, ref newArtist, ref newGenre);
                track.IndexingSuccess = 1;
            }
            catch (Exception ex)
            {
                track.IndexingFailureReason = ex.Message;
                LogClient.Error("Error while retrieving tag information for file {0}. Exception: {1}", track.Path, ex.Message);
            }

            if (track.IndexingSuccess == 1)
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
        }

        private async void WatcherManager_FoldersChanged(object sender, EventArgs e)
        {
            await this.RefreshCollectionAsync();
        }

        public event EventHandler IndexingStopped = delegate { };
        public event EventHandler IndexingStarted = delegate { };
        public event Action<IndexingStatusEventArgs> IndexingStatusChanged = delegate { };
        public event EventHandler RefreshLists = delegate { };
        public event EventHandler RefreshArtwork = delegate { };
    }
}
