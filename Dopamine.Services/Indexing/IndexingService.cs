using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Utils;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Data.Metadata;
using Dopamine.Services.Contracts.Cache;
using Dopamine.Services.Contracts.Indexing;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Indexing
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
        private List<FolderPathInfo> allDiskPaths;
        private List<FolderPathInfo> newDiskPaths;

        // Cache
        private IndexerCache cache;

        // Factory
        private ISQLiteConnectionFactory factory;

        // Flags
        private bool isIndexing;
        private bool isFoldersChanged;
        private bool canIndexArtwork;
        private bool isIndexingArtwork;

        // Events
        public event EventHandler IndexingStopped = delegate { };
        public event EventHandler IndexingStarted = delegate { };
        public event Action<IndexingStatusEventArgs> IndexingStatusChanged = delegate { };
        public event EventHandler RefreshLists = delegate { };
        public event EventHandler RefreshArtwork = delegate { };
        public event AlbumArtworkAddedEventHandler AlbumArtworkAdded = delegate { };

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
            this.cache = new IndexerCache(this.factory);

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

            this.isFoldersChanged = false;
            await this.RefreshCollectionAsync();
        }

        public async Task RefreshCollectionImmediatelyAsync()
        {
            await this.CheckCollectionAsync(true);
        }

        private async Task CheckCollectionAsync(bool forceIndexing)
        {
            if (this.IsIndexing)
            {
                return;
            }

            LogClient.Info("+++ STARTED CHECKING COLLECTION +++");

            this.canIndexArtwork = false;

            // Wait until artwork indexing is stopped
            while (this.isIndexingArtwork)
            {
                await Task.Delay(100);
            }

            await this.watcherManager.StopWatchingAsync();

            try
            {
                this.allDiskPaths = await this.GetFolderPaths();

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
                        long diskLastDateFileModified = this.allDiskPaths.Count > 0 ? this.allDiskPaths.Select((t) => t.DateModifiedTicks).OrderByDescending((t) => t).First() : 0;
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
                            this.AddArtworkInBackgroundAsync();
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

            // Artwork cleanup
            // ---------------
            bool isArtworkCleanedUp = await this.CleanupArtworkAsync();

            // Refresh lists
            // -------------
            if (isTracksChanged || isArtworkCleanedUp)
            {
                LogClient.Info("Sending event to refresh the lists because: isTracksChanged = {0}, isArtworkCleanedUp = {1}", isTracksChanged, isArtworkCleanedUp);
                this.RefreshLists(this, new EventArgs());
            }

            // Finalize
            // --------
            this.isIndexing = false;
            this.IndexingStopped(this, new EventArgs());

            this.AddArtworkInBackgroundAsync();

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

                await this.GetNewDiskPathsAsync(ignoreRemovedFiles); // Obsolete tracks are removed, now we can determine new files.
                this.cache.Initialize(); // After obsolete tracks are removed, we can initialize the cache.

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

                this.newDiskPaths = new List<FolderPathInfo>();

                foreach (FolderPathInfo diskpath in this.allDiskPaths)
                {
                    if (!dbPaths.Contains(diskpath.Path.ToSafePath()) && (ignoreRemovedFiles ? !removedPaths.Contains(diskpath.Path.ToSafePath()) : true))
                    {
                        this.newDiskPaths.Add(diskpath);
                    }
                }
            });
        }

        private async Task<long> RemoveTracksAsync()
        {
            long numberRemovedTracks = 0;

            var args = new IndexingStatusEventArgs()
            {
                IndexingAction = IndexingAction.RemoveTracks,
                ProgressPercent = 0
            };

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        // Create a list of folderIDs
                        List<long> folderTrackIDs = conn.Table<FolderTrack>().ToList().Select((t) => t.TrackID).Distinct().ToList();

                        List<Track> alltracks = conn.Table<Track>().Select((t) => t).ToList();
                        List<Track> tracksInMissingFolders = alltracks.Select((t) => t).Where(t => !folderTrackIDs.Contains(t.TrackID)).ToList();
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
                            this.IndexingStatusChanged(args);

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
                                        this.IndexingStatusChanged(args);
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

            var args = new IndexingStatusEventArgs()
            {
                IndexingAction = IndexingAction.UpdateTracks,
                ProgressPercent = 0
            };

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
                        int lastPercent = 0;

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

                            int percent = IndexerUtils.CalculatePercent(currentValue, totalValue);

                            // Report progress if at least 1 track is updated OR when the progress
                            // interval has been exceeded OR the maximum has been reached.
                            bool mustReportProgress = numberUpdatedTracks == 1 || percent >= lastPercent + 5 || percent == 100;

                            if (mustReportProgress)
                            {
                                lastPercent = percent;
                                args.ProgressPercent = percent;
                                this.IndexingStatusChanged(args);
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

            var args = new IndexingStatusEventArgs()
            {
                IndexingAction = IndexingAction.AddTracks,
                ProgressPercent = 0
            };

            await Task.Run(() =>
            {
                try
                {
                    long currentValue = 0;
                    long totalValue = this.newDiskPaths.Count;

                    long saveItemCount = IndexerUtils.CalculateSaveItemCount(totalValue);
                    long unsavedItemCount = 0;
                    int lastPercent = 0;

                    using (var conn = this.factory.GetConnection())
                    {
                        conn.BeginTransaction();

                        foreach (FolderPathInfo newDiskPath in this.newDiskPaths)
                        {
                            Track diskTrack = Track.CreateDefault(newDiskPath.Path);

                            try
                            {
                                this.ProcessTrack(diskTrack, conn);

                                if (!this.cache.HasCachedTrack(ref diskTrack))
                                {
                                    conn.Insert(diskTrack);
                                    this.cache.AddTrack(diskTrack);
                                    numberAddedTracks += 1;
                                    unsavedItemCount += 1;
                                }

                                conn.Insert(new FolderTrack(newDiskPath.FolderId, diskTrack.TrackID));

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
                                LogClient.Error("There was a problem while adding Track with path='{0}'. Exception: {1}", diskTrack.Path, ex.Message);
                            }

                            currentValue += 1;

                            int percent = IndexerUtils.CalculatePercent(currentValue, totalValue);

                            // Report progress if at least 1 track is added OR when the progress
                            // interval has been exceeded OR the maximum has been reached.
                            bool mustReportProgress = numberAddedTracks == 1 || percent >= lastPercent + 5 || percent == 100;

                            if (mustReportProgress)
                            {
                                lastPercent = percent;
                                args.ProgressCurrent = numberAddedTracks;
                                args.ProgressPercent = percent;
                                this.IndexingStatusChanged(args);
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
            var newAlbum = new Album() { NeedsIndexing = 1 }; // Make sure album art gets indexed for this album
            var newArtist = new Artist();
            var newGenre = new Genre();

            try
            {
                MetadataUtils.SplitMetadata(track.Path, ref track, ref newTrackStatistic, ref newAlbum, ref newArtist, ref newGenre);

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

                        // A track from this album has changed, so make sure album art gets re-indexed.
                        dbAlbum.NeedsIndexing = 1;
                        conn.Update(dbAlbum);
                    }
                }

                track.IndexingSuccess = 1;
                track.AlbumID = newAlbum.AlbumID;
                track.ArtistID = newArtist.ArtistID;
                track.GenreID = newGenre.GenreID;
            }
            catch (Exception ex)
            {
                // When updating tracks: for tracks that were indexed successfully in the past and had IndexingSuccess = 1
                track.IndexingSuccess = 0;
                track.AlbumID = 0;
                track.ArtistID = 0;
                track.GenreID = 0;
                track.IndexingFailureReason = ex.Message;

                LogClient.Error("Error while retrieving tag information for file {0}. Exception: {1}", track.Path, ex.Message);
            }

            return;
        }

        private async void WatcherManager_FoldersChanged(object sender, EventArgs e)
        {
            await this.RefreshCollectionAsync();
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
                    }
                }
            });

            return numberDeleted;
        }

        private async Task<bool> CleanupArtworkAsync()
        {
            LogClient.Info("+++ STARTED CLEANING UP ARTWORK +++");

            DateTime startTime = DateTime.Now;
            long numberDeletedFromDatabase = 0;
            long numberDeletedFromDisk = 0;

            try
            {
                // Step 1: delete unused artwork from the database
                // -----------------------------------------------
                numberDeletedFromDatabase = await this.DeleteUnusedArtworkFromDatabaseAsync();

                // Step 2: delete unused artwork from the cache
                // --------------------------------------------
                numberDeletedFromDisk = await this.DeleteUnusedArtworkFromCacheAsync();
            }
            catch (Exception ex)
            {
                LogClient.Info("There was a problem while updating the artwork. Exception: {0}", ex.Message);
            }

            LogClient.Info("+++ FINISHED CLEANING UP ARTWORK: Covers deleted from database: {0}. Covers deleted from disk: {1}. Time required: {3} ms +++", numberDeletedFromDatabase, numberDeletedFromDisk, Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));

            return numberDeletedFromDatabase + numberDeletedFromDisk > 0;
        }

        private async Task<string> GetArtworkFromFile(Album album)
        {
            Track trk = this.GetLastModifiedTrack(album);
            return await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(album, trk.Path));
        }

        private async Task<string> GetArtworkFromInternet(Album album)
        {
            Uri artworkUri = await ArtworkUtils.GetAlbumArtworkFromInternetAsync(album.AlbumTitle, album.AlbumArtist);
            return await this.cacheService.CacheArtworkAsync(artworkUri);
        }

        private async void AddArtworkInBackgroundAsync()
        {
            // First, add artwork from file.
            await this.AddArtworkInBackgroundAsync(1);

            // Next, add artwork from the Internet, if the user has chosen to do so.
            if (SettingsClient.Get<bool>("Covers", "DownloadMissingAlbumCovers"))
            {
                // Add artwork from the Internet.
                await this.AddArtworkInBackgroundAsync(2);
            }
            else
            {
                // Don't add artwork from the Internet. Mark all albums as indexed.
                await this.MarkAllAlbumsAsIndexed();
            }
        }

        private async Task MarkAllAlbumsAsIndexed()
        {
            await this.albumRepository.SetAlbumsNeedsIndexing(0, false);
        }

        private async Task AddArtworkInBackgroundAsync(int passNumber)
        {
            LogClient.Info("+++ STARTED ADDING ARTWORK IN THE BACKGROUND +++");
            this.canIndexArtwork = true;
            this.isIndexingArtwork = true;

            DateTime startTime = DateTime.Now;

            await Task.Run(async () =>
            {
                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    try
                    {
                        conn.BeginTransaction();

                        List<long> albumIdsWithArtwork = new List<long>();
                        List<Album> albumsToIndex = conn.Table<Album>().ToList().Where(a => a.NeedsIndexing == 1).ToList();

                        foreach (Album alb in albumsToIndex)
                        {
                            if (!this.canIndexArtwork)
                            {
                                try
                                {
                                    LogClient.Info("+++ ABORTED ADDING ARTWORK IN THE BACKGROUND. Time required: {0} ms +++", Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));
                                    conn.Commit(); // Makes sure we commit what we already processed
                                    this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumIds = albumIdsWithArtwork }); // Update UI
                                }
                                catch (Exception ex)
                                {
                                    LogClient.Error("Failed to commit changes while aborting adding artwork in background. Exception: {0}", ex.Message);
                                }

                                this.isIndexingArtwork = false;

                                return;
                            }

                            try
                            {
                                if (passNumber.Equals(1))
                                {
                                    // During the 1st pass, look for artwork in file(s).
                                    // Only set NeedsIndexing = 0 if artwork was found. If no artwork was found, 
                                    // this gives the 2nd pass a chance to look for artwork on the Internet.
                                    alb.ArtworkID = await this.GetArtworkFromFile(alb);

                                    if (!string.IsNullOrEmpty(alb.ArtworkID))
                                    {
                                        alb.NeedsIndexing = 0;
                                    }
                                }
                                else if (passNumber.Equals(2))
                                {
                                    // During the 2nd pass, look for artwork on the Internet and set alb.NeedsIndexing = 0.
                                    // We don't want future passes to index this album anymore.
                                    alb.ArtworkID = await this.GetArtworkFromInternet(alb);
                                    alb.NeedsIndexing = 0;
                                }

                                // If artwork was found, keep track of the albumID
                                if (!string.IsNullOrEmpty(alb.ArtworkID))
                                {
                                    albumIdsWithArtwork.Add(alb.AlbumID);
                                }

                                alb.DateLastSynced = DateTime.Now.Ticks;
                                conn.Update(alb);

                                // If artwork was found for 20 albums, trigger a refresh of the UI.
                                if (albumIdsWithArtwork.Count >= 20)
                                {
                                    conn.Commit(); // Commit, because the UI refresh will need up to date albums in the database.
                                    await Task.Delay(1000); // Hopefully prevents database locks
                                    List<long> eventAlbumIds = new List<long>(albumIdsWithArtwork);
                                    albumIdsWithArtwork.Clear();
                                    this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumIds = eventAlbumIds }); // Update UI
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while updating the cover art for Album {0}/{1}. Exception: {2}", alb.AlbumTitle, alb.AlbumArtist, ex.Message);
                            }
                        }

                        try
                        {
                            conn.Commit(); // Make sure all albums are committed
                            this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumIds = albumIdsWithArtwork }); // Update UI
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Failed to commit changes while finishing adding artwork in background. Exception: {0}", ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Unexpected error occurred while updating artwork in the background. Exception: {0}", ex.Message);
                    }
                }
            });

            this.isIndexingArtwork = false;
            LogClient.Error("+++ FINISHED ADDING ARTWORK IN THE BACKGROUND. Time required: {0} ms +++", Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));
        }

        public async void ReloadAlbumArtworkAsync(bool onlyUpdateWhenNoCover)
        {
            this.canIndexArtwork = false;

            // Wait until artwork indexing is stopped
            while (this.isIndexingArtwork)
            {
                await Task.Delay(100);
            }

            await this.albumRepository.SetAlbumsNeedsIndexing(1, onlyUpdateWhenNoCover);

            this.AddArtworkInBackgroundAsync();
        }

        private async Task<List<FolderPathInfo>> GetFolderPaths()
        {
            var allFolderPaths = new List<FolderPathInfo>();
            List<Folder> folders = await this.folderRepository.GetFoldersAsync();

            await Task.Run(() =>
            {
                // Recursively get all the files in the collection folders
                foreach (Folder fol in folders)
                {
                    if (Directory.Exists(fol.Path))
                    {
                        try
                        {
                            // Get all audio files recursively
                            List<FolderPathInfo> folderPaths = FileOperations.GetValidFolderPaths(fol.FolderID, fol.Path, FileFormats.SupportedMediaExtensions, SearchOption.AllDirectories);
                            allFolderPaths.AddRange(folderPaths);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Error while recursively getting files/folders for directory={0}. Exception: {1}", fol.Path, ex.Message);
                        }
                    }
                }
            });

            return allFolderPaths;
        }
    }
}
