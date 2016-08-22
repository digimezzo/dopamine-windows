using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using SQLite.Net;
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
        // Directories
        private string cacheSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheSubDirectory);
        private string coverArtCacheSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheSubDirectory, ApplicationPaths.CoverArtCacheSubDirectory);

        // Repositories
        private ITrackRepository trackRepository;
        private IFolderRepository folderRepository;
        private IAlbumRepository albumRepository;
        private IArtistRepository artistRepository;
        private IGenreRepository genreRepository;
        private IPlaylistEntryRepository playlistEntryRepository;

        // Paths
        private List<Tuple<long, string, long>> allDiskPaths;
        private List<Tuple<long, string, long>> newDiskPaths;

        // Folders
        List<long> unreachableFolderIDs;

        // Cache
        private IndexerCache cache;

        // Context
        private IndexingContext context;

        // Factory
        private SQLiteConnectionFactory factory;

        // IndexingEventArgs
        private IndexingStatusEventArgs eventArgs;

        // Flags
        private bool isIndexing;
        private bool needsIndexing;
        #endregion

        #region Properties
        public bool IsIndexing
        {
            get { return this.isIndexing; }
        }

        public bool NeedsIndexing
        {
            get { return this.needsIndexing; }
            set { this.needsIndexing = value; }
        }
        #endregion

        #region Construction
        public IndexingService(ITrackRepository trackRepository, IAlbumRepository albumRepository, IGenreRepository genreRepository, IArtistRepository artistRepository, IFolderRepository folderRepository, IPlaylistEntryRepository playlistEntryRepository)
        {
            // Initialize Repositories
            // -----------------------
            this.trackRepository = trackRepository;
            this.albumRepository = albumRepository;
            this.genreRepository = genreRepository;
            this.artistRepository = artistRepository;
            this.folderRepository = folderRepository;
            this.playlistEntryRepository = playlistEntryRepository;

            // Initialize the Cache directory and its subdirectories
            // -----------------------------------------------------
            // If the Cache subdirectory doesn't exist, create it
            if (!Directory.Exists(this.cacheSubDirectory))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(this.cacheSubDirectory));
            }

            // Delete old cache directories
            if (Directory.Exists(System.IO.Path.Combine(this.cacheSubDirectory, "CoverPictures")))
            {
                Directory.Delete(System.IO.Path.Combine(this.cacheSubDirectory, "CoverPictures"), true);
            }

            if (Directory.Exists(System.IO.Path.Combine(this.cacheSubDirectory, "CoverThumbnails")))
            {
                Directory.Delete(System.IO.Path.Combine(this.cacheSubDirectory, "CoverThumbnails"), true);
            }

            // Create the cache directory
            if (!Directory.Exists(this.coverArtCacheSubDirectory))
            {
                Directory.CreateDirectory(this.coverArtCacheSubDirectory);
            }

            // Initialize values
            // -----------------
            this.needsIndexing = true;
            this.isIndexing = false;
        }
        #endregion

        #region IIndexingService
        public async Task CheckCollectionAsync(bool ignoreRemovedFiles, bool artworkOnly)
        {
            if (this.IsIndexing | !this.needsIndexing) return;
            else this.needsIndexing = false;

            await this.InitializeAsync();

            string lastFileCountString = string.Empty;
            string lastDateFileModifiedString = string.Empty;

            using (var conn = this.factory.GetConnection())
            {
                lastFileCountString = conn.Table<IndexingStatistic>().Where((t) => t.Key.Equals("LastFileCount")).Select((t) => t.Value).FirstOrDefault();
                lastDateFileModifiedString = conn.Table<IndexingStatistic>().Where((t) => t.Key.Equals("LastDateFileModified")).Select((t) => t.Value).FirstOrDefault();
            }

            long lastFileCount = 0;
            long lastDateFileModified = 0;

            long.TryParse(lastFileCountString, out lastFileCount);
            long.TryParse(lastDateFileModifiedString, out lastDateFileModified);

            if (lastFileCount != this.allDiskPaths.Count | (this.allDiskPaths.Count > 0 && (lastDateFileModified < this.allDiskPaths.Select((t) => t.Item3).OrderByDescending((t) => t).First())))
            {
                this.needsIndexing = true;
                await this.IndexCollectionAsync(ignoreRemovedFiles, artworkOnly, true);
            }
        }

        public async Task IndexCollectionAsync(bool ignoreRemovedFiles, bool artworkOnly, bool isInitialized = false)
        {
            if (this.IsIndexing | !this.needsIndexing) return;
            else this.needsIndexing = false;

            this.isIndexing = true;

            this.IndexingStarted(this, new EventArgs());

            // Initialize
            // ----------
            if (!isInitialized) await this.InitializeAsync();

            // Tracks
            // ------
            if (!artworkOnly)
            {
                this.eventArgs.IndexingDataChanged = await this.IndexTracksAsync(ignoreRemovedFiles) > 0 ? IndexingDataChanged.Tracks : IndexingDataChanged.None;

                if (this.eventArgs.IndexingDataChanged == IndexingDataChanged.Tracks)
                {
                    LogClient.Instance.Logger.Info("Sending event to refresh the lists");
                    this.IndexingStatusChanged(this.eventArgs);
                    this.RefreshLists(this, new EventArgs());
                    this.eventArgs.IndexingDataChanged = IndexingDataChanged.None;
                }
            }

            // Artwork
            // -------
            this.eventArgs.IndexingDataChanged = await this.IndexArtworkAsync(!artworkOnly) > 0 ? IndexingDataChanged.Artwork : IndexingDataChanged.None;

            if (this.eventArgs.IndexingDataChanged == IndexingDataChanged.Artwork)
            {
                LogClient.Instance.Logger.Info("Sending event to refresh the artwork");
                this.IndexingStatusChanged(this.eventArgs);
                this.RefreshArtwork(this, new EventArgs());
                this.eventArgs.IndexingDataChanged = IndexingDataChanged.None;
            }

            // Statistics
            // ----------
            await this.UpdateIndexingStatisticsAsync();

            this.isIndexing = false;

            this.IndexingStatusChanged(this.eventArgs);
            this.IndexingStopped(this, new EventArgs());
        }
        #endregion

        #region Private
        private async Task InitializeAsync()
        {
            // Initialize Factory
            this.factory = new SQLiteConnectionFactory();

            // Initialize Cache
            this.cache = new IndexerCache();

            // Initialize Context
            this.context = new IndexingContext();

            // IndexingEventArgs
            this.eventArgs = new IndexingStatusEventArgs();
            this.eventArgs.IndexingDataChanged = IndexingDataChanged.None;
            this.eventArgs.IndexingAction = IndexingAction.Idle;

            // Get all files on disk which belong to a Collection Folder
            this.allDiskPaths = await this.folderRepository.GetPathsAsync();

            // Find unreachable Folders
            this.unreachableFolderIDs = new List<long>();

            var folders = await this.folderRepository.GetFoldersAsync();

            foreach (Folder fol in folders)
            {
                if (!Directory.Exists(fol.Path))
                {
                    this.unreachableFolderIDs.Add(fol.FolderID);
                }
            }
        }

        private async Task UpdateIndexingStatisticsAsync()
        {
            await Task.Run(() =>
            {
                using (var conn = this.factory.GetConnection())
                {
                    IndexingStatistic lastFileCountStatistic = conn.Table<IndexingStatistic>().Select((t) => t).Where((t) => t.Key.Equals("LastFileCount")).FirstOrDefault();
                    IndexingStatistic lastDateFileModifiedStatistic = conn.Table<IndexingStatistic>().Select((t) => t).Where((t) => t.Key.Equals("LastDateFileModified")).FirstOrDefault();

                    long currentDateFileModified = this.allDiskPaths.Select(t => t.Item3).OrderByDescending(t => t).FirstOrDefault();
                    currentDateFileModified = currentDateFileModified > 0 ? currentDateFileModified : 0;

                    long currentFileCount = this.allDiskPaths.Count;

                    if (lastFileCountStatistic != null)
                    {
                        lastFileCountStatistic.Value = currentFileCount.ToString();
                        conn.Update(lastFileCountStatistic);
                    }
                    else
                    {
                        conn.Insert(new IndexingStatistic { Key = "LastFileCount", Value = currentFileCount.ToString() });
                    }

                    if (lastDateFileModifiedStatistic != null)
                    {
                        lastDateFileModifiedStatistic.Value = currentDateFileModified.ToString();
                        conn.Update(lastDateFileModifiedStatistic);
                    }
                    else
                    {
                        conn.Insert(new IndexingStatistic { Key = "LastDateFileModified", Value = currentDateFileModified.ToString() });
                    }
                }
            });
        }

        private async Task<long> IndexArtworkAsync(bool quickArtworkIndexing)
        {
            LogClient.Instance.Logger.Info("+++ STARTED INDEXING ARTWORK +++");

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
                numberUpdated = await this.AddArtworkAsync(quickArtworkIndexing);

                // Step 3: delete unused artwork from the cache
                // --------------------------------------------
                numberDeletedFromDisk = await this.DeleteUnusedArtworkFromCacheAsync();
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Info("There was a problem while updating the artwork. Exception: {0}", ex.Message);
            }

            LogClient.Instance.Logger.Info("+++ FINISHED INDEXING ARTWORK: Covers deleted from database: {0}. Covers deleted from disk: {1}. Covers updated: {2}. Time required: {3} ms +++", numberDeletedFromDatabase, numberDeletedFromDisk, numberUpdated, Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));

            return numberDeletedFromDatabase + numberDeletedFromDisk + numberUpdated;
        }

        private async Task<long> AddArtworkAsync(bool quickArtworkIndexing = true)
        {
            long numberUpdated = 0;

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    foreach (Album alb in conn.Table<Album>())
                    {
                        try
                        {
                            // Only update artwork if QuickArtworkIndexing is enabled AND there 
                            // is no ArtworkID set, OR when QuickArtworkIndexing is disabled.
                            if ((quickArtworkIndexing & string.IsNullOrEmpty(alb.ArtworkID)) | !quickArtworkIndexing)
                            {
                                Track trk = this.GetLastModifiedTrack(alb);

                                if (IndexerUtils.CacheArtwork(alb, trk.Path))
                                {
                                    this.context.Update<Album>(alb);
                                    numberUpdated += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("There was a problem while updating the cover art for Album {0}/{1}. Exception: {2}", alb.AlbumTitle, alb.AlbumArtist, ex.Message);
                        }

                        // Report progress if at least 1 album is added
                        if (numberUpdated > 0)
                        {
                            this.eventArgs.IndexingAction = IndexingAction.UpdateArtwork;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);
                        }
                    }

                    if (numberUpdated > 0)
                    {
                        this.context.SaveChanges();
                    }
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
                    foreach (Album alb in conn.Table<Album>().Where(t => !string.IsNullOrEmpty(t.ArtworkID)))
                    {
                        if (!System.IO.File.Exists(ArtworkUtils.GetArtworkPath(alb)))
                        {
                            alb.ArtworkID = string.Empty;
                            this.context.Update<Album>(alb);
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

                    if (numberDeleted > 0)
                    {
                        this.context.SaveChanges();
                    }
                }
            });

            return numberDeleted;
        }

        private async Task<long> DeleteUnusedArtworkFromCacheAsync()
        {
            long numberDeleted = 0;

            await Task.Run(() =>
            {
                string[] artworkFiles = Directory.GetFiles(System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheSubDirectory, ApplicationPaths.CoverArtCacheSubDirectory), "album-*.jpg");

                using (SQLiteConnection conn = this.factory.GetConnection())
                {
                    List<String> artworkIDs = conn.Table<Album>().Where((t) => !string.IsNullOrEmpty(t.ArtworkID)).Select((t) => t.ArtworkID).ToList();

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
                                LogClient.Instance.Logger.Error("There was a problem while deleting cached artwork {0}. Exception: {1}", artworkFile, ex.Message);
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
            LogClient.Instance.Logger.Info("+++ STARTED INDEXING COLLECTION +++");

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

                LogClient.Instance.Logger.Info("Tracks removed: {0}. Time required: {1} ms +++", numberTracksRemoved, Convert.ToInt64(DateTime.Now.Subtract(removeTracksStartTime).TotalMilliseconds));

                await this.GetNewDiskPathsAsync(ignoreRemovedFiles); // Obsolete Tracks are removed, now we can determine new files

                // Step 2: update outdated Tracks
                // ------------------------------
                DateTime updateTracksStartTime = DateTime.Now;
                numberTracksUpdated = await this.UpdateTracksAsync();

                LogClient.Instance.Logger.Info("Tracks updated: {0}. Time required: {1} ms +++", numberTracksUpdated, Convert.ToInt64(DateTime.Now.Subtract(updateTracksStartTime).TotalMilliseconds));

                // Step 3: add new Tracks
                // ----------------------
                DateTime addTracksStartTime = DateTime.Now;
                numberTracksAdded = await this.AddTracksAsync();

                LogClient.Instance.Logger.Info("Tracks added: {0}. Time required: {1} ms +++", numberTracksAdded, Convert.ToInt64(DateTime.Now.Subtract(addTracksStartTime).TotalMilliseconds));

                // Step 4: delete orphans
                // ----------------------

                await this.albumRepository.DeleteOrphanedAlbumsAsync(); // Delete orphaned Albums
                await this.artistRepository.DeleteOrphanedArtistsAsync(); // Delete orphaned Artists
                await this.genreRepository.DeleteOrphanedGenresAsync(); // Delete orphaned Genres
                await this.playlistEntryRepository.DeleteOrphanedPlaylistEntriesAsync(); // Delete orphaned PlaylistEntries

                // Step 5: compact the database
                // ----------------------------
                using (var conn = factory.GetConnection())
                {
                    conn.Execute("VACUUM;");
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Info("There was a problem while indexing the collection. Exception: {0}", ex.Message);
            }

            LogClient.Instance.Logger.Info("+++ FINISHED INDEXING COLLECTION: Tracks removed: {0}. Tracks updated: {1}. Tracks added: {2}. Time required: {3} ms +++", numberTracksRemoved, numberTracksUpdated, numberTracksAdded, Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds));

            return numberTracksRemoved + numberTracksAdded + numberTracksUpdated;
        }

        private async Task GetNewDiskPathsAsync(bool ignoreRemovedFiles)
        {
            await Task.Run(() =>
            {
                var dbPaths = new List<string>();

                using (var conn = this.factory.GetConnection())
                {
                    dbPaths = conn.Table<Track>().Select((trk) => trk.Path).ToList();
                }

                var removedPaths = new List<string>();

                using (var conn = this.factory.GetConnection())
                {
                    removedPaths = conn.Table<RemovedTrack>().Select((t) => t.Path).ToList();
                }

                this.newDiskPaths = new List<Tuple<long, string, long>>();

                foreach (Tuple<long, string, long> diskpath in this.allDiskPaths)
                {
                    if (!dbPaths.Contains(diskpath.Item2) && (ignoreRemovedFiles ? !removedPaths.Contains(diskpath.Item2) : true))
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
                        List<long> folderIDs = conn.Table<Folder>().Select((t) => t.FolderID).ToList();

                        // Ignore Tracks which are in an unreachable folder
                        List<Track> tracksToProcess = conn.Table<Track>().Select((t) => t).Where(t => !this.unreachableFolderIDs.Contains(t.FolderID)).ToList();
                        List<Track> tracksToProcessInvalidFolderID = tracksToProcess.Where(t => !folderIDs.Contains(t.FolderID)).ToList();
                        List<Track> tracksToProcessValidFolderID = tracksToProcess.Except(tracksToProcessInvalidFolderID).ToList();

                        // Process tracks with an invalid FolderID

                        if (tracksToProcessInvalidFolderID.Count > 0)
                        {
                            // Report progress
                            this.eventArgs.IndexingAction = IndexingAction.RemoveTracks;
                            this.eventArgs.ProgressPercent = 0;
                            this.IndexingStatusChanged(this.eventArgs);

                            // Delete
                            conn.Delete(tracksToProcessInvalidFolderID);
                            numberRemovedTracks += tracksToProcessInvalidFolderID.Count;
                        }

                        // Process tracks with a valid FolderID
                        foreach (Track trk in tracksToProcessValidFolderID)
                        {
                            if (!System.IO.File.Exists(trk.Path) | !folderIDs.Contains(trk.FolderID))
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
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("There was a problem while removing Tracks. Exception: {0}", ex.Message);
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
                        // Ignore Tracks which are in an unreachable folder
                        List<Track> tracksToProcess = conn.Table<Track>().Select((t) => t).Where((t) => !this.unreachableFolderIDs.Contains(t.FolderID)).ToList();

                        long currentValue = 0;
                        long totalValue = tracksToProcess.Count;

                        foreach (Track dbTrack in tracksToProcess)
                        {
                            try
                            {
                                if (IndexerUtils.IsTrackOutdated(dbTrack))
                                {
                                    if (this.ProcessTrack(dbTrack))
                                    {
                                        context.Update<Track>(dbTrack);
                                        numberUpdatedTracks += 1;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("There was a problem while updating Track with path='{0}'. Exception: {1}", dbTrack.Path, ex.Message);
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

                        this.context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("There was a problem while updating Tracks. Exception: {0}", ex.Message);
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
                            using (var conn = this.factory.GetConnection())
                            {
                                if (this.ProcessTrack(diskTrack))
                                {
                                    this.context.Insert<Track>(diskTrack);
                                    numberAddedTracks += 1;
                                    unsavedItemCount += 1;
                                }

                                // Intermediate save to the database if 20% is reached
                                if (unsavedItemCount == saveItemCount)
                                {
                                    unsavedItemCount = 0;
                                    this.context.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("There was a problem while updating Track with path='{0}'. Exception: {1}", diskTrack.Path, ex.Message);
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

                    // Final save
                    this.context.SaveChanges();
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("There was a problem while adding Tracks. Exception: {0}", ex.Message);
                }
            });

            return numberAddedTracks;
        }

        private bool ProcessTrack(Track track)
        {
            bool processingSuccessful = false;

            var newAlbum = new Album();
            var newArtist = new Artist();
            var newGenre = new Genre();

            try
            {
                IndexerUtils.SplitMetadata(track.Path, ref track, ref newAlbum, ref newArtist, ref newGenre);
                processingSuccessful = true;
            }
            catch (Exception ex)
            {
                processingSuccessful = false;
                LogClient.Instance.Logger.Error("Error while retrieving tag information for file {0}. File not added to the database. Exception: {1}", track.Path, ex.Message);
            }

            if (processingSuccessful)
            {
                // Check if such Artist already exists in the database
                if (!this.cache.GetCachedArtist(ref newArtist))
                {
                    // If not, add it.
                    this.context.Insert<Artist>(newArtist);
                }

                // Check if such Genre already exists in the database 
                if (!this.cache.GetCachedGenre(ref newGenre))
                {
                    // If not, add it.
                    this.context.Insert<Genre>(newGenre);
                }

                // Check if such Album already exists in the database
                if (!this.cache.GetCachedAlbum(ref newAlbum))
                {
                    // If Not, add it.
                    this.context.Insert<Album>(newAlbum);
                }
                else
                {
                    // Make sure the Year of the existing album is updated
                    // TODO: can we prevent a database query here?
                    using (SQLiteConnection conn = this.factory.GetConnection())
                    {
                        Album dbAlbum = conn.Table<Album>().Where((a) => a.AlbumID.Equals(newAlbum.AlbumID)).FirstOrDefault();

                        if (dbAlbum != null)
                        {
                            dbAlbum.Year = newAlbum.Year;
                            context.Update<Album>(dbAlbum);
                        }
                    }
                }

                track.AlbumID = newAlbum.AlbumID;
                track.ArtistID = newArtist.ArtistID;
                track.GenreID = newGenre.GenreID;
            }

            return processingSuccessful;
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
