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
        private Queue<FileMetadata> fileMetadatas;
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
        public event Action<MetadataChangedEventArgs> MetadataChanged;
        public event Action<RatingChangedEventArgs> RatingChanged;
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

            this.fileMetadatas = new Queue<FileMetadata>();

            this.updateFileMetadataTimer = new Timer();
            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout;
            this.updateFileMetadataTimer.Elapsed += DelayedUpdateFileMetadataHandler;

            this.playbackService.PlaybackStopped += DelayedUpdateFileMetadataHandler;
            this.playbackService.PlaybackFailed += DelayedUpdateFileMetadataHandler;
            this.playbackService.PlaybackSuccess += DelayedUpdateFileMetadataHandler;
        }
        #endregion

        #region IMetadataService
        public async Task UpdateTrackRatingAsync(string path, int rating)
        {
            Track dbTrack = await this.trackRepository.GetTrackAsync(path);

            // Update datebase track rating only if the track can be found
            if (dbTrack != null)
            {
                dbTrack.Rating = rating;
                await this.trackRepository.UpdateTrackAsync(dbTrack);
            }

            // Update the rating in the file if the user selected this option
            if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "SaveRatingToAudioFiles"))
            {
                // Only for MP3's
                if (Path.GetExtension(path).ToLower().Equals(FileFormats.MP3.ToLower()))
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

        public async Task UpdateTrackAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            // Update the metadata in the database immediately
            await this.UpdateDatabaseMetadataAsync(fileMetadatas, updateAlbumArtwork);

            // Queue update of the file metadata
            await this.QueueFileMetadata(fileMetadatas);
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

            if (updateFileArtwork)
            {
                List<TrackInfo> albumTracks = await this.trackRepository.GetTracksAsync(album.ToList());
                List<FileMetadata> fileMetadatas = (from t in albumTracks select new FileMetadata(t.Path) { ArtworkData = artwork }).ToList();

                // Queue update of the file metadata
                await this.QueueFileMetadata(fileMetadatas);
            }

            // Raise event
            var metadataChangedEventArgs = new MetadataChangedEventArgs();
            metadataChangedEventArgs.IsAlbumArtworkMetadataChanged = true;
            this.MetadataChanged(metadataChangedEventArgs);
        }

        public async Task UpdateFilemetadataAsync()
        {
            this.updateFileMetadataTimer.Stop();

            this.isUpdatingFileMetadata = true;

            var localFileMetadatas = new Queue<FileMetadata>();
            var successfulFileMetadatas = new List<FileMetadata>();
            var failedFileMetadatas = new List<FileMetadata>();

            await Task.Run(() =>
            {
                // Create a local collection of FileMetadata's
                lock (lockObject)
                {
                    while (this.fileMetadatas.Count > 0)
                    {
                        localFileMetadatas.Enqueue(this.fileMetadatas.Dequeue());
                    }
                }

                // Process local FileMetadata's
                while (localFileMetadatas.Count > 0)
                {
                    FileMetadata fmd = localFileMetadatas.Dequeue();

                    try
                    {
                        fmd.Save();
                        if (!successfulFileMetadatas.Contains(fmd)) successfulFileMetadatas.Add(fmd);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Unable to save metadata to the file for Track '{0}'. Exception: {1}", fmd.FileName, ex.Message);

                        try
                        {
                            LogClient.Instance.Logger.Error("Trying to save metadata for Track '{0}' after suspending playback. Exception: {1}", fmd.FileName, ex.Message);

                            this.playbackService.Suspend();
                            fmd.Save();
                            if (!successfulFileMetadatas.Contains(fmd)) successfulFileMetadatas.Add(fmd);
                            this.playbackService.Unsuspend();
                        }
                        catch (Exception)
                        {
                            LogClient.Instance.Logger.Error("Unable to save metadata to the file for Track '{0}' after suspending playback. Metadata saving is now queued for later. Exception: {1}", fmd.FileName, ex.Message);
                            failedFileMetadatas.Add(fmd);
                        }
                    }
                }
            });

            // Make sure failed FileMetadata's are processed again the next time the timer elapses
            if (failedFileMetadatas.Count > 0) await this.QueueFileMetadata(failedFileMetadatas);

            // Sync file size and last modified date in the database
            foreach (FileMetadata fmd in successfulFileMetadatas)
            {
                await this.trackRepository.UpdateTrackFileInformationAsync(fmd.FileName);
            }

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout; // The next time, wait longer.

            this.isUpdatingFileMetadata = false;

            this.updateFileMetadataTimer.Start();
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
                        this.fileMetadatas.Enqueue(fmd);
                    }
                }
            });

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataShortTimeout; // The next time, almost don't wait.
            this.updateFileMetadataTimer.Start();
        }

        private async Task<bool> UpdateDatabaseTrackMetadataAsync(FileMetadata fileMetadata)
        {

            bool isMetadataChanged = false;

            Track dbTrack = await this.trackRepository.GetTrackAsync(fileMetadata.FileName);

            if (fileMetadata.Title.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.TrackTitle = fileMetadata.Title.Value;
            }

            if (fileMetadata.Year.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.Year = fileMetadata.Year.Value.SafeConvertToLong();
            }

            if (fileMetadata.TrackNumber.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.TrackNumber = fileMetadata.TrackNumber.Value.SafeConvertToLong();
            }

            if (fileMetadata.TrackCount.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.TrackCount = fileMetadata.TrackCount.Value.SafeConvertToLong();
            }

            if (fileMetadata.DiscNumber.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.DiscNumber = fileMetadata.DiscNumber.Value.SafeConvertToLong();
            }

            if (fileMetadata.DiscCount.IsValueChanged)
            {
                isMetadataChanged = true;
                dbTrack.DiscCount = fileMetadata.DiscCount.Value.SafeConvertToLong();
            }

            if (isMetadataChanged) await this.trackRepository.UpdateTrackAsync(dbTrack);

            return isMetadataChanged;
        }

        private async Task<bool> UpdateDatabaseArtistMetadataAsync(FileMetadata fileMetadata)
        {
            bool isMetadataChanged = false;

            Track dbTrack = await this.trackRepository.GetTrackAsync(fileMetadata.FileName);

            if (fileMetadata.Artists.IsValueChanged)
            {
                isMetadataChanged = true;
                Artist dbArtist = null;
                string newArtistName = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownArtistString;
                dbArtist = await this.artistRepository.GetArtistAsync(newArtistName);
                if (dbArtist == null) dbArtist = await this.artistRepository.AddArtistAsync(new Artist { ArtistName = newArtistName });
                if (dbArtist != null) dbTrack.ArtistID = dbArtist.ArtistID;

                await this.trackRepository.UpdateTrackAsync(dbTrack);
            }

            return isMetadataChanged;
        }

        private async Task<bool> UpdateDatabaseGenreMetadataAsync(FileMetadata fmd)
        {
            bool isMetadataChanged = false;

            Track dbTrack = await this.trackRepository.GetTrackAsync(fmd.FileName);

            if (fmd.Genres.IsValueChanged)
            {
                isMetadataChanged = true;
                Genre dbGenre = null;
                string newGenreName = fmd.Genres.Values != null && !string.IsNullOrEmpty(fmd.Genres.Values.FirstOrDefault()) ? fmd.Genres.Values.FirstOrDefault() : Defaults.UnknownGenreString;
                dbGenre = await this.genreRepository.GetGenreAsync(newGenreName);
                if (dbGenre == null) dbGenre = await this.genreRepository.AddGenreAsync(new Genre { GenreName = newGenreName });
                if (dbGenre != null) dbTrack.GenreID = dbGenre.GenreID;

                await this.trackRepository.UpdateTrackAsync(dbTrack);
            }

            return isMetadataChanged;
        }

        private async Task<AlbumMetadataChangeStatus> UpdateDatabaseAlbumMetadataAsync(FileMetadata fileMetadata, bool updateAlbumArtwork)
        {
            var albumMetadataChangeStatus = new AlbumMetadataChangeStatus();

            Track dbTrack = await this.trackRepository.GetTrackAsync(fileMetadata.FileName);

            if (fileMetadata.Album.IsValueChanged | fileMetadata.AlbumArtists.IsValueChanged | fileMetadata.Year.IsValueChanged)
            {
                albumMetadataChangeStatus.IsAlbumTitleChanged = fileMetadata.Album.IsValueChanged;
                albumMetadataChangeStatus.IsAlbumArtistChanged = fileMetadata.AlbumArtists.IsValueChanged;
                albumMetadataChangeStatus.IsAlbumYearChanged = fileMetadata.Year.IsValueChanged;

                Album dbAlbum = null;
                string newAlbumTitle = !string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumString;
                string newAlbumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : Defaults.UnknownAlbumArtistString;

                dbAlbum = await this.albumRepository.GetAlbumAsync(newAlbumTitle, newAlbumArtist);

                if (dbAlbum == null)
                {
                    dbAlbum = new Album { AlbumTitle = newAlbumTitle, AlbumArtist = newAlbumArtist, DateLastSynced = DateTime.Now.Ticks };

                    dbAlbum.ArtworkID = await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(dbAlbum, dbTrack.Path));

                    dbAlbum = await this.albumRepository.AddAlbumAsync(dbAlbum);
                }

                dbTrack.AlbumID = dbAlbum.AlbumID;
                await this.trackRepository.UpdateTrackAsync(dbTrack);

                await Task.Run(() => IndexerUtils.UpdateAlbumYear(dbAlbum, fileMetadata.Year.Value.SafeConvertToLong())); // Update the album's year
                await this.albumRepository.UpdateAlbumAsync(dbAlbum);
            }

            if (updateAlbumArtwork)
            {
                albumMetadataChangeStatus.IsAlbumArtworkChanged = true;

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

            return albumMetadataChangeStatus;
        }

        private async Task UpdateDatabaseMetadataAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            this.isUpdatingDatabaseMetadata = true;

            var metadataChangedEventArgs = new MetadataChangedEventArgs();

            foreach (FileMetadata fmd in fileMetadatas)
            {
                try
                {
                    metadataChangedEventArgs.IsTrackMetadataChanged = await this.UpdateDatabaseTrackMetadataAsync(fmd);
                    metadataChangedEventArgs.IsArtistMetadataChanged = await this.UpdateDatabaseArtistMetadataAsync(fmd);
                    metadataChangedEventArgs.IsGenreMetadataChanged = await this.UpdateDatabaseGenreMetadataAsync(fmd);

                    AlbumMetadataChangeStatus albumMetadataChangeStatus = await this.UpdateDatabaseAlbumMetadataAsync(fmd, updateAlbumArtwork);
                    metadataChangedEventArgs.IsAlbumTitleMetadataChanged = albumMetadataChangeStatus.IsAlbumTitleChanged;
                    metadataChangedEventArgs.IsAlbumArtistMetadataChanged = albumMetadataChangeStatus.IsAlbumArtistChanged;
                    metadataChangedEventArgs.IsAlbumYearMetadataChanged = albumMetadataChangeStatus.IsAlbumYearChanged;
                    metadataChangedEventArgs.IsAlbumArtworkMetadataChanged = albumMetadataChangeStatus.IsAlbumArtworkChanged;
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Unable to update database metadata for Track '{0}'. Exception: {1}", fmd.FileName, ex.Message);
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

                this.MetadataChanged(metadataChangedEventArgs);
            }

            this.isUpdatingDatabaseMetadata = false;
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
