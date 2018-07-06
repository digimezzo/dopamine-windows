using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Utils;
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
        private IAlbumArtworkRepository albumArtworkRepository;
        private IFolderRepository folderRepository;

        // Factories
        private ISQLiteConnectionFactory factory;
        private IFileMetadataFactory fileMetadataFactory;

        // Watcher
        private FolderWatcherManager watcherManager;

        // Paths
        private List<FolderPathInfo> allDiskPaths;
        private List<FolderPathInfo> newDiskPaths;

        // Cache
        private IndexerCache cache;

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
            IFolderRepository folderRepository, IFileMetadataFactory fileMetadataFactory, IAlbumArtworkRepository albumArtworkRepository)
        {
            this.cacheService = cacheService;
            this.trackRepository = trackRepository;
            this.folderRepository = folderRepository;
            this.albumArtworkRepository = albumArtworkRepository;
            this.factory = factory;
            this.fileMetadataFactory = fileMetadataFactory;

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
            try
            {
                MetadataUtils.FillTrack(this.fileMetadataFactory.Create(track.Path), ref track);

                track.IndexingSuccess = 1;

                // Make sure that we check for album cover art
                track.NeedsAlbumArtworkIndexing = 1;
            }
            catch (Exception ex)
            {
                // When updating tracks: for tracks that were indexed successfully in the past and had IndexingSuccess = 1
                track.IndexingSuccess = 0;
                track.IndexingFailureReason = ex.Message;

                LogClient.Error("Error while retrieving tag information for file {0}. Exception: {1}", track.Path, ex.Message);
            }

            return;
        }

        private async void WatcherManager_FoldersChanged(object sender, EventArgs e)
        {
            await this.RefreshCollectionAsync();
        }

        private async Task<long> DeleteUnusedArtworkFromCacheAsync()
        {
            long numberDeleted = 0;

            await Task.Run(async() =>
            {
                string[] artworkFiles = Directory.GetFiles(this.cacheService.CoverArtCacheFolderPath, "album-*.jpg");

                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    IList<string> artworkIds = await this.albumArtworkRepository.GetArtworkIdsAsync();

                    foreach (string artworkFile in artworkFiles)
                    {
                        if (!artworkIds.Contains(Path.GetFileNameWithoutExtension(artworkFile)))
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
                // Step 1: delete unused AlbumArtwork from the database (Which isn't mapped to a Track's AlbumKey)
                // -----------------------------------------------------------------------------------------------
                numberDeletedFromDatabase = await this.albumArtworkRepository.DeleteUnusedAlbumArtworkAsync();

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

        private async Task<string> GetArtworkFromFile(string albumKey)
        {
            Track track = await this.trackRepository.GetLastModifiedTrackForAlbumKeyAsync(albumKey);
            return await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(albumKey, this.fileMetadataFactory.Create(track.Path)));
        }

        private async Task<string> GetArtworkFromInternet(string albumTitle, IList<string> albumArtists)
        {
            Uri artworkUri = await ArtworkUtils.GetAlbumArtworkFromInternetAsync(albumTitle, albumArtists);
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

            // We don't need to scan for artwork anymore
            await this.trackRepository.DisableNeedsAlbumArtworkIndexingForAllTracksAsync();
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
                        IList<string> albumKeysWithArtwork = new List<string>();
                        IList<AlbumData> albumDatasToIndex = await this.trackRepository.GetAlbumDataToIndexAsync();

                        foreach (AlbumData albumDataToIndex in albumDatasToIndex)
                        {
                            // Check if we must cancel artwork indexing
                            if (!this.canIndexArtwork)
                            {
                                try
                                {
                                    LogClient.Info("+++ ABORTED ADDING ARTWORK IN THE BACKGROUND. Time required: {0} ms +++", Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));
                                    this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumKeys = albumKeysWithArtwork }); // Update UI
                                }
                                catch (Exception ex)
                                {
                                    LogClient.Error("Failed to commit changes while aborting adding artwork in background. Exception: {0}", ex.Message);
                                }

                                this.isIndexingArtwork = false;

                                return;
                            }

                            // Start indexing artwork
                            try
                            {
                                // Delete existing AlbumArtwork
                                await this.albumArtworkRepository.DeleteAlbumArtworkAsync(albumDataToIndex.AlbumKey);

                                // Create a new AlbumArtwork
                                var albumArtwork = new AlbumArtwork(albumDataToIndex.AlbumKey);

                                if (passNumber.Equals(1))
                                {
                                    // During the 1st pass, look for artwork in file(s).
                                    // Only set NeedsAlbumArtworkIndexing = 0 if artwork was found. So when no artwork was found, 
                                    // this gives the 2nd pass a chance to look for artwork on the Internet.
                                    albumArtwork.ArtworkID = await this.GetArtworkFromFile(albumDataToIndex.AlbumKey);

                                    if (!string.IsNullOrEmpty(albumArtwork.ArtworkID))
                                    {
                                        await this.trackRepository.DisableNeedsAlbumArtworkIndexingAsync(albumDataToIndex.AlbumKey);
                                    }
                                }
                                else if (passNumber.Equals(2))
                                {
                                    // During the 2nd pass, look for artwork on the Internet and set NeedsAlbumArtworkIndexing = 0.
                                    // We don't want future passes to index for this AlbumKey anymore.
                                    albumArtwork.ArtworkID = await this.GetArtworkFromInternet(
                                        albumDataToIndex.AlbumTitle, 
                                        MetadataUtils.GetMultiValueTagsCollection(albumDataToIndex.AlbumArtists).ToList()
                                        );

                                    await this.trackRepository.DisableNeedsAlbumArtworkIndexingAsync(albumDataToIndex.AlbumKey);
                                }

                                // If artwork was found, keep track of the albumID
                                if (!string.IsNullOrEmpty(albumArtwork.ArtworkID))
                                {
                                    albumKeysWithArtwork.Add(albumArtwork.AlbumKey);
                                    conn.Insert(albumArtwork);
                                }

                                // If artwork was found for 20 albums, trigger a refresh of the UI.
                                if (albumKeysWithArtwork.Count >= 20)
                                {
                                    var eventAlbumKeys = new List<string>(albumKeysWithArtwork);
                                    albumKeysWithArtwork.Clear();
                                    this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumKeys = eventAlbumKeys }); // Update UI
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("There was a problem while updating the cover art for Album {0}/{1}. Exception: {2}", albumDataToIndex.AlbumTitle, albumDataToIndex.AlbumArtists, ex.Message);
                            }
                        }

                        try
                        {
                            this.AlbumArtworkAdded(this, new AlbumArtworkAddedEventArgs() { AlbumKeys = albumKeysWithArtwork }); // Update UI
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

        public async void ReScanAlbumArtworkAsync(bool onlyWhenHasNoCover)
        {
            this.canIndexArtwork = false;

            // Wait until artwork indexing is stopped
            while (this.isIndexingArtwork)
            {
                await Task.Delay(100);
            }

            await this.trackRepository.EnableNeedsAlbumArtworkIndexingForAllTracksAsync(onlyWhenHasNoCover);

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
                            List<FolderPathInfo> folderPaths = FileOperations.GetValidFolderPaths(fol.FolderID, fol.Path, FileFormats.SupportedMediaExtensions);
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
