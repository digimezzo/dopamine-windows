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
using System.Diagnostics;
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
                    var fmd = new FileMetadata(path);
                    fmd.Rating = new MetadataRatingValue() { Value = rating };
                    await this.QueueUpdateFileMetadata(new FileMetadata[] { fmd }.ToList());
                }
            }

            this.RatingChanged(new RatingChangedEventArgs { Path = path, Rating = rating });
        }

        public async Task UpdateTrackLoveAsync(string path, bool love)
        {
            Track track = await this.trackRepository.GetTrackAsync(path);

            // Update datebase track love only if the track can be found
            if (track != null)
            {
                track.Love = love ? 1 : 0;
                await this.trackRepository.UpdateTrackAsync(track);
            }

            this.LoveChanged(new LoveChangedEventArgs { Path = path, Love = love });
        }

        public async Task UpdateTracksAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            var args = new MetadataChangedEventArgs();

            // Update the metadata in the database
            await this.UpdateDatabaseMetadataAsync(fileMetadatas, updateAlbumArtwork);

            // Update the metadata in the PlaybackService
            await this.playbackService.UpdateQueueMetadataAsync(fileMetadatas);

            // Queue update of the file metadata
            await this.QueueUpdateFileMetadata(fileMetadatas);

            foreach (FileMetadata fmd in fileMetadatas)
            {
                if (fmd.IsArtistMetadataChanged) args.IsArtistChanged = true;
                if (fmd.IsGenreMetadataChanged) args.IsGenreChanged = true;
                if (fmd.IsAlbumMetadataChanged) args.IsAlbumChanged = true;
                if (fmd.IsTrackMetadataChanged) args.IsTrackChanged = true;
                if (fmd.ArtworkData.IsValueChanged) args.IsArtworkChanged = true;
            }

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

            List<MergedTrack> albumTracks = await this.trackRepository.GetTracksAsync(album.ToList());
            List<FileMetadata> fileMetadatas = (from t in albumTracks select new FileMetadata(t.Path) { ArtworkData = artwork }).ToList();

            // Update the metadata in the PlaybackService
            await this.playbackService.UpdateQueueMetadataAsync(fileMetadatas);

            // Queue update of the file metadata
            if (updateFileArtwork) await this.QueueUpdateFileMetadata(fileMetadatas);
            
            var args = new MetadataChangedEventArgs() { IsArtworkChanged = true };

            // Raise event
            this.MetadataChanged(args);
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
        private async Task QueueUpdateFileMetadata(List<FileMetadata> fileMetadatas)
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

        private async Task UpdateDatabaseTrackMetadataAsync(FileMetadata fileMetadata)
        {
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (track == null) return;

            if (fileMetadata.IsTrackMetadataChanged)
            {
                if (fileMetadata.Title.IsValueChanged) track.TrackTitle = fileMetadata.Title.Value;
                if (fileMetadata.Year.IsValueChanged) track.Year = fileMetadata.Year.Value.SafeConvertToLong();
                if (fileMetadata.TrackNumber.IsValueChanged) track.TrackNumber = fileMetadata.TrackNumber.Value.SafeConvertToLong();
                if (fileMetadata.TrackCount.IsValueChanged) track.TrackCount = fileMetadata.TrackCount.Value.SafeConvertToLong();
                if (fileMetadata.DiscNumber.IsValueChanged) track.DiscNumber = fileMetadata.DiscNumber.Value.SafeConvertToLong();
                if (fileMetadata.DiscCount.IsValueChanged) track.DiscCount = fileMetadata.DiscCount.Value.SafeConvertToLong();
                if (fileMetadata.Lyrics.IsValueChanged) Debug.WriteLine("Lyrics are not saved in the database");

                await this.trackRepository.UpdateTrackAsync(track);
            }
        }

        private async Task UpdateDatabaseArtistMetadataAsync(FileMetadata fileMetadata)
        {
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (track == null) return;

            if (fileMetadata.IsArtistMetadataChanged)
            {
                Artist artist = null;
                string newArtistName = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownArtistString;
                artist = await this.artistRepository.GetArtistAsync(newArtistName);
                if (artist == null) artist = await this.artistRepository.AddArtistAsync(new Artist { ArtistName = newArtistName });
                if (artist != null) track.ArtistID = artist.ArtistID;

                await this.trackRepository.UpdateTrackAsync(track);
            }
        }

        private async Task UpdateDatabaseGenreMetadataAsync(FileMetadata fileMetadata)
        {
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (track == null) return;

            if (fileMetadata.IsGenreMetadataChanged)
            {
                Genre genre = null;
                string newGenreName = fileMetadata.Genres.Values != null && !string.IsNullOrEmpty(fileMetadata.Genres.Values.FirstOrDefault()) ? fileMetadata.Genres.Values.FirstOrDefault() : Defaults.UnknownGenreString;
                genre = await this.genreRepository.GetGenreAsync(newGenreName);
                if (genre == null) genre = await this.genreRepository.AddGenreAsync(new Genre { GenreName = newGenreName });
                if (genre != null) track.GenreID = genre.GenreID;

                await this.trackRepository.UpdateTrackAsync(track);
            }
        }

        private async Task UpdateDatabaseAlbumMetadataAsync(FileMetadata fileMetadata, bool updateAlbumArtwork)
        {
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (track == null) return;

            if (fileMetadata.IsAlbumMetadataChanged)
            {
                Album album = null;
                string newAlbumTitle = !string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumString;
                string newAlbumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : Defaults.UnknownAlbumArtistString;

                album = await this.albumRepository.GetAlbumAsync(newAlbumTitle, newAlbumArtist);

                if (album == null)
                {
                    album = new Album { AlbumTitle = newAlbumTitle, AlbumArtist = newAlbumArtist, DateLastSynced = DateTime.Now.Ticks };
                    album.ArtworkID = await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(album, track.Path));
                    album = await this.albumRepository.AddAlbumAsync(album);
                }

                track.AlbumID = album.AlbumID;

                await this.trackRepository.UpdateTrackAsync(track);
                await Task.Run(() => IndexerUtils.UpdateAlbumYear(album, fileMetadata.Year.Value.SafeConvertToLong())); // Update the album's year
                await this.albumRepository.UpdateAlbumAsync(album);
            }

            if (updateAlbumArtwork)
            {
                string artworkID = String.Empty;

                try
                {
                    artworkID = await this.cacheService.CacheArtworkAsync(fileMetadata.ArtworkData.Value);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while caching artwork data", ex.Message);
                }

                string albumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : string.Empty;

                // If no album artist is found, use the artist name. The album was probably saved using the artist name.
                if (string.IsNullOrEmpty(albumArtist))
                {
                    albumArtist = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownAlbumArtistString;
                }

                await this.albumRepository.UpdateAlbumArtworkAsync(!string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumString,
                                                            albumArtist,
                                                            artworkID);
            }
        }

        private async Task UpdateDatabaseMetadataAsync(List<FileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            this.isUpdatingDatabaseMetadata = true;
            bool mustDeleteOrphans = false;

            foreach (FileMetadata fmd in fileMetadatas)
            {
                try
                {
                    await this.UpdateDatabaseTrackMetadataAsync(fmd);
                    await this.UpdateDatabaseArtistMetadataAsync(fmd);
                    await this.UpdateDatabaseGenreMetadataAsync(fmd);
                    await this.UpdateDatabaseAlbumMetadataAsync(fmd, updateAlbumArtwork);

                    if (fmd.IsMetadataChanged) mustDeleteOrphans = true;
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Unable to update database metadata for Track '{0}'. Exception: {1}", fmd.SafePath, ex.Message);
                }
            }

            if (mustDeleteOrphans)
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
