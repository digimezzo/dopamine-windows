using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        #region Variables
        private ITrackRepository trackRepository;
        private IAlbumRepository albumRepository;
        private IGenreRepository genreRepository;
        private IArtistRepository artistRepository;
        private bool isUpdatingDatabaseMetadata;
        private bool isUpdatingFileMetadata;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private Queue<FileMetadata> queuedFileMetadatas;
        private object lockObject = new object();
        private Timer updateFileMetadataTimer;
        private int updateFileMetadataShortTimeout = 250; // 250 milliseconds
        private int updateFileMetadataLongTimeout = 15000; // 15 seconds
        #endregion

        #region ReadOnly Property
        public bool IsUpdatingDatabaseMetadata
        {
            get { return this.isUpdatingDatabaseMetadata; }
        }

        public bool IsUpdatingFileMetadata
        {
            get { return this.isUpdatingFileMetadata; }
        }
        #endregion

        #region Events
        public event Action<MetadataChangedEventArgs> MetadataChanged = delegate { };
        public event Action<RatingChangedEventArgs> RatingChanged = delegate { };
        public event Action<LoveChangedEventArgs> LoveChanged = delegate { };
        #endregion

        #region Construction
        public MetadataService(ICacheService cacheService, IPlaybackService playbackService, ITrackRepository trackRepository, IAlbumRepository albumRepository, IGenreRepository genreRepository, IArtistRepository artistRepository)
        {
            this.cacheService = cacheService;
            this.playbackService = playbackService;

            this.trackRepository = trackRepository;
            this.albumRepository = albumRepository;
            this.genreRepository = genreRepository;
            this.artistRepository = artistRepository;

            this.queuedFileMetadatas = new Queue<FileMetadata>();

            this.updateFileMetadataTimer = new Timer();
            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout;
            this.updateFileMetadataTimer.Elapsed += DelayedUpdateFileMetadataHandler;

            this.playbackService.PlaybackStopped += DelayedUpdateFileMetadataHandler;
            this.playbackService.PlaybackFailed += DelayedUpdateFileMetadataHandler;
            this.playbackService.PlaybackSuccess += DelayedUpdateFileMetadataHandler;
        }
        #endregion

        #region IMetadataService
        public async Task<FileMetadata> GetFileMetadataAsync(string path)
        {
            bool restartTimer = this.updateFileMetadataTimer.Enabled; // If the timer is started, remember to restart it once we're done here.
            this.updateFileMetadataTimer.Stop();

            FileMetadata returnFileMetadata = null;

            await Task.Run(() =>
            {
                // Check if there is a queued FileMetadata for this path, if yes, use that as it has more up to date information.
                lock (lockObject)
                {
                    foreach (FileMetadata fmd in this.queuedFileMetadatas)
                    {
                        if (fmd.SafePath == path.ToSafePath())
                        {
                            returnFileMetadata = fmd;
                        }
                    }
                }
            });

            // If no queued FileMetadata was found, create a new one from the actual file.
            if (returnFileMetadata == null) returnFileMetadata = new FileMetadata(path);
            if (restartTimer) this.updateFileMetadataTimer.Start(); // Restart the timer if necessary

            return returnFileMetadata;
        }

        public async Task UpdateTrackRatingAsync(string path, int rating)
        {
            Track track = await this.trackRepository.GetTrackAsync(path);

            // Update datebase track rating only if the track can be found
            if (track != null)
            {
                track.Rating = rating;
                await this.trackRepository.UpdateTrackAsync(track);
            }

            // Update the rating in the file if the user selected this option
            if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "SaveRatingToAudioFiles"))
            {
                // Only for MP3's
                if (Path.GetExtension(path).ToLower().Equals(FileFormats.MP3))
                {
                    var metadataRatingValue = new MetadataRatingValue();
                    metadataRatingValue.Value = rating;

                    var fmd = new FileMetadata(path);
                    fmd.Rating = metadataRatingValue;
                    await this.QueueFileMetadata(new FileMetadata[] { fmd }.ToList());
                }
            }

            this.RatingChanged(new RatingChangedEventArgs { Path = path, Rating = rating });
        }

        public async Task UpdateTrackLoveAsync(string path, bool love)
        {
            Track track = await this.trackRepository.GetTrackAsync(path);

            // Update datebase track rating only if the track can be found
            if (track != null)
            {
                track.Love = love ? 1 : 0;
                await this.trackRepository.UpdateTrackAsync(track);
            }

            this.LoveChanged(new LoveChangedEventArgs { Path = path, Love = love });
        }

        public async Task UpdateTrackAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            // Update the metadata in the database
            MetadataChangedEventArgs args = await this.UpdateDatabaseMetadataAsync(fileMetadatas, updateAlbumArtwork);

            // Queue update of the file metadata
            await this.QueueFileMetadata(fileMetadatas);

            // Find the changed paths
            List<string> changedPaths = null;
            await Task.Run(() => { changedPaths = fileMetadatas.Select(f => f.Path).ToList(); });
            args.ChangedPaths = changedPaths;

            // Raise event
            this.MetadataChanged(args);
        }

        public async Task UpdateAlbumAsync(Album album, MetadataArtworkValue artwork, bool updateFileArtwork)
        {
            string artworkID = String.Empty;

            // Cache new artwork
            if (artwork != null)
            {
                try
                {
                    artworkID = await this.cacheService.CacheArtworkAsync(artwork.Value);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while caching artwork data for album with title='{0}' and album artist='{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
                }
            }

            // Update artwork in database
            await this.albumRepository.UpdateAlbumArtworkAsync(album.AlbumTitle, album.AlbumArtist, artworkID);

            List<MergedTrack> albumTracks = await this.trackRepository.GetMergedTracksAsync(album.ToList());
            List<FileMetadata> fileMetadatas = (from t in albumTracks select new FileMetadata(t.Path) { ArtworkData = artwork }).ToList();

            if (updateFileArtwork)
            {
                // Queue update of the file metadata
                await this.QueueFileMetadata(fileMetadatas);
            }

            // Find the changed paths
            List<string> changedPaths = null;
            await Task.Run(() => { changedPaths = fileMetadatas.Select(f => f.Path).ToList(); });

            // Raise event
            this.MetadataChanged(new MetadataChangedEventArgs() { IsArtworkChanged = true, ChangedPaths = changedPaths });
        }

        public async Task UpdateFilemetadataAsync()
        {
            bool restartTimer = false;
            this.updateFileMetadataTimer.Stop();

            this.isUpdatingFileMetadata = true;

            var filesToSync = new List<FileMetadata>();

            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    var failedFileMetadatas = new Queue<FileMetadata>();

                    while (this.queuedFileMetadatas.Count > 0)
                    {
                        FileMetadata fmd = this.queuedFileMetadatas.Dequeue();

                        try
                        {
                            fmd.Save();
                            filesToSync.Add(fmd);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Unable to save metadata to the file for Track '{0}'. Exception: {1}", fmd.SafePath, ex.Message);
                            failedFileMetadatas.Enqueue(fmd);
                        }
                    }

                    // Make sure failed FileMetadata's are processed the next time the timer elapses
                    if (failedFileMetadatas.Count > 0)
                    {
                        restartTimer = true; // If there are still queued FileMetadata's, start the timer.

                        foreach (FileMetadata fmd in failedFileMetadatas)
                        {
                            this.queuedFileMetadatas.Enqueue(fmd);
                        }
                    }
                }
            });

            // Sync file size and last modified date in the database
            foreach (FileMetadata fmd in filesToSync)
            {
                await this.trackRepository.UpdateTrackFileInformationAsync(fmd.SafePath);
            }

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout; // The next time, wait longer.

            this.isUpdatingFileMetadata = false;

            if (restartTimer) this.updateFileMetadataTimer.Start();
        }
        #endregion

        #region Private
        private async Task QueueFileMetadata(List<FileMetadata> fileMetadatas)
        {
            this.updateFileMetadataTimer.Stop();

            await Task.Run(() =>
            {
                lock (this.lockObject)
                {
                    foreach (FileMetadata fmd in fileMetadatas)
                    {
                        this.queuedFileMetadatas.Enqueue(fmd);
                    }
                }
            });

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataShortTimeout; // The next time, almost don't wait.
            this.updateFileMetadataTimer.Start();
        }

        private async Task<bool> UpdateDatabaseTrackMetadataAsync(FileMetadata fileMetadata)
        {

            bool isMetadataChanged = false;

            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (fileMetadata.Title.IsValueChanged)
            {
                isMetadataChanged = true;
                track.TrackTitle = fileMetadata.Title.Value;
            }

            if (fileMetadata.Year.IsValueChanged)
            {
                isMetadataChanged = true;
                track.Year = fileMetadata.Year.Value.SafeConvertToLong();
            }

            if (fileMetadata.TrackNumber.IsValueChanged)
            {
                isMetadataChanged = true;
                track.TrackNumber = fileMetadata.TrackNumber.Value.SafeConvertToLong();
            }

            if (fileMetadata.TrackCount.IsValueChanged)
            {
                isMetadataChanged = true;
                track.TrackCount = fileMetadata.TrackCount.Value.SafeConvertToLong();
            }

            if (fileMetadata.DiscNumber.IsValueChanged)
            {
                isMetadataChanged = true;
                track.DiscNumber = fileMetadata.DiscNumber.Value.SafeConvertToLong();
            }

            if (fileMetadata.DiscCount.IsValueChanged)
            {
                isMetadataChanged = true;
                track.DiscCount = fileMetadata.DiscCount.Value.SafeConvertToLong();
            }

            if (isMetadataChanged) await this.trackRepository.UpdateTrackAsync(track);

            if (fileMetadata.Lyrics.IsValueChanged)
            {
                isMetadataChanged = true;
                // Lyrics are not saved in the database. We only need to set "isMetadataChanged = true" here.
            }

            return isMetadataChanged;
        }

        private async Task<bool> UpdateDatabaseArtistMetadataAsync(FileMetadata fileMetadata)
        {
            bool isMetadataChanged = false;

            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (fileMetadata.Artists.IsValueChanged)
            {
                isMetadataChanged = true;
                Artist dbArtist = null;
                string newArtistName = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownArtistString;
                dbArtist = await this.artistRepository.GetArtistAsync(newArtistName);
                if (dbArtist == null) dbArtist = await this.artistRepository.AddArtistAsync(new Artist { ArtistName = newArtistName });
                if (dbArtist != null) track.ArtistID = dbArtist.ArtistID;

                await this.trackRepository.UpdateTrackAsync(track);
            }

            return isMetadataChanged;
        }

        private async Task<bool> UpdateDatabaseGenreMetadataAsync(FileMetadata fmd)
        {
            bool isMetadataChanged = false;

            Track track = await this.trackRepository.GetTrackAsync(fmd.SafePath);

            if (fmd.Genres.IsValueChanged)
            {
                isMetadataChanged = true;
                Genre dbGenre = null;
                string newGenreName = fmd.Genres.Values != null && !string.IsNullOrEmpty(fmd.Genres.Values.FirstOrDefault()) ? fmd.Genres.Values.FirstOrDefault() : Defaults.UnknownGenreString;
                dbGenre = await this.genreRepository.GetGenreAsync(newGenreName);
                if (dbGenre == null) dbGenre = await this.genreRepository.AddGenreAsync(new Genre { GenreName = newGenreName });
                if (dbGenre != null) track.GenreID = dbGenre.GenreID;

                await this.trackRepository.UpdateTrackAsync(track);
            }

            return isMetadataChanged;
        }

        private async Task<bool> UpdateDatabaseAlbumMetadataAsync(FileMetadata fileMetadata, bool updateAlbumArtwork)
        {
            bool isMetadataChanged = false;

            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (fileMetadata.Album.IsValueChanged | fileMetadata.AlbumArtists.IsValueChanged | fileMetadata.Year.IsValueChanged)
            {
                isMetadataChanged = true;
                Album dbAlbum = null;
                string newAlbumTitle = !string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumString;
                string newAlbumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : Defaults.UnknownAlbumArtistString;

                dbAlbum = await this.albumRepository.GetAlbumAsync(newAlbumTitle, newAlbumArtist);

                if (dbAlbum == null)
                {
                    dbAlbum = new Album { AlbumTitle = newAlbumTitle, AlbumArtist = newAlbumArtist, DateLastSynced = DateTime.Now.Ticks };

                    dbAlbum.ArtworkID = await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(dbAlbum, track.Path));

                    dbAlbum = await this.albumRepository.AddAlbumAsync(dbAlbum);
                }

                track.AlbumID = dbAlbum.AlbumID;
                await this.trackRepository.UpdateTrackAsync(track);

                await Task.Run(() => IndexerUtils.UpdateAlbumYear(dbAlbum, fileMetadata.Year.Value.SafeConvertToLong())); // Update the album's year
                await this.albumRepository.UpdateAlbumAsync(dbAlbum);
            }

            if (updateAlbumArtwork)
            {
                isMetadataChanged = true;

                string artworkID = String.Empty;

                try
                {
                    artworkID = await this.cacheService.CacheArtworkAsync(fileMetadata.ArtworkData.Value);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while caching artwork data", ex.Message);
                }

                await this.albumRepository.UpdateAlbumArtworkAsync(!string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumString,
                                                            fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : Defaults.UnknownAlbumArtistString,
                                                            artworkID);
            }

            return isMetadataChanged;
        }

        private async Task<MetadataChangedEventArgs> UpdateDatabaseMetadataAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            this.isUpdatingDatabaseMetadata = true;

            var metadataChangedEventArgs = new MetadataChangedEventArgs();

            foreach (FileMetadata fmd in fileMetadatas)
            {
                try
                {
                    metadataChangedEventArgs.IsTrackChanged = await this.UpdateDatabaseTrackMetadataAsync(fmd);
                    metadataChangedEventArgs.IsArtistChanged = await this.UpdateDatabaseArtistMetadataAsync(fmd);
                    metadataChangedEventArgs.IsGenreChanged = await this.UpdateDatabaseGenreMetadataAsync(fmd);
                    metadataChangedEventArgs.IsAlbumChanged = await this.UpdateDatabaseAlbumMetadataAsync(fmd, updateAlbumArtwork);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Unable to update database metadata for Track '{0}'. Exception: {1}", fmd.SafePath, ex.Message);
                }
            }

            if (metadataChangedEventArgs.IsMetadataChanged)
            {
                try
                {
                    await this.albumRepository.DeleteOrphanedAlbumsAsync();
                    await this.genreRepository.DeleteOrphanedGenresAsync();
                    await this.artistRepository.DeleteOrphanedArtistsAsync();
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Error while deleting orphans. Exception: {0}", ex.Message);
                }
            }

            this.isUpdatingDatabaseMetadata = false;

            return metadataChangedEventArgs;
        }

        private async void DelayedUpdateFileMetadataHandler(Object sender, EventArgs e)
        {
            await this.UpdateFilemetadataAsync();
        }

        private async void DelayedUpdateFileMetadataHandler(bool isPlayingPreviousTrack)
        {
            await this.UpdateFilemetadataAsync();
        }
        #endregion
    }
}
