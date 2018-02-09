using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Metadata;
using Dopamine.Data.Contracts.Repositories;
using Dopamine.Services.Contracts.Cache;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Indexing;
using Dopamine.Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        private ITrackRepository trackRepository;
        private ITrackStatisticRepository trackStatisticRepository;
        private IAlbumRepository albumRepository;
        private IGenreRepository genreRepository;
        private IArtistRepository artistRepository;
        private IFileMetadataFactory metadataFactory;
        private bool isUpdatingDatabaseMetadata;
        private bool isUpdatingFileMetadata;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private Dictionary<string, IFileMetadata> fileMetadataDictionary;
        private object lockObject = new object();
        private Timer updateFileMetadataTimer;
        private int updateFileMetadataShortTimeout = 50; // 50 milliseconds
        private int updateFileMetadataLongTimeout = 15000; // 15 seconds
        private Tuple<string, byte[]> cachedArtwork;
        private object cachedArtworkLock = new object();

        public bool IsUpdatingDatabaseMetadata
        {
            get { return this.isUpdatingDatabaseMetadata; }
        }

        public event Action<MetadataChangedEventArgs> MetadataChanged = delegate { };
        public event Action<RatingChangedEventArgs> RatingChanged = delegate { };
        public event Action<LoveChangedEventArgs> LoveChanged = delegate { };

        public MetadataService(ICacheService cacheService, IPlaybackService playbackService, ITrackRepository trackRepository,
            ITrackStatisticRepository trackStatisticRepository, IAlbumRepository albumRepository, IGenreRepository genreRepository,
            IArtistRepository artistRepository, IFileMetadataFactory metadataFactory)
        {
            this.cacheService = cacheService;
            this.playbackService = playbackService;

            this.trackStatisticRepository = trackStatisticRepository;
            this.trackRepository = trackRepository;
            this.albumRepository = albumRepository;
            this.genreRepository = genreRepository;
            this.artistRepository = artistRepository;
            this.metadataFactory = metadataFactory;

            this.fileMetadataDictionary = new Dictionary<string, IFileMetadata>();

            this.updateFileMetadataTimer = new Timer();
            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout;
            this.updateFileMetadataTimer.Elapsed += async (_, __) => await this.UpdateFileMetadataAsync();

            this.playbackService.PlaybackStopped += async (_, __) => await this.UpdateFileMetadataAsync();
            this.playbackService.PlaybackFailed += async (_, __) => await this.UpdateFileMetadataAsync();
            this.playbackService.PlaybackSuccess += async (_, __) => await this.UpdateFileMetadataAsync();
        }

        public IFileMetadata GetFileMetadata(string path)
        {
            bool restartTimer = this.updateFileMetadataTimer.Enabled; // If the timer is started, remember to restart it once we're done here.
            this.updateFileMetadataTimer.Stop();

            IFileMetadata returnFileMetadata = null;

            // Check if there is a queued FileMetadata for this path, if yes, use that as it has more up to date information.
            lock (lockObject)
            {
                if (this.fileMetadataDictionary.ContainsKey(path.ToSafePath()))
                {
                    returnFileMetadata = this.fileMetadataDictionary[path.ToSafePath()];
                }
            }

            // If no queued FileMetadata was found, create a new one from the actual file.
            if (returnFileMetadata == null) returnFileMetadata = metadataFactory.Create(path);
            if (restartTimer) this.updateFileMetadataTimer.Start(); // Restart the timer if necessary

            return returnFileMetadata;
        }

        public async Task<IFileMetadata> GetFileMetadataAsync(string path)
        {
            IFileMetadata returnFileMetadata = null;

            await Task.Run(() => { returnFileMetadata = this.GetFileMetadata(path); });

            return returnFileMetadata;
        }

        public async Task UpdateTrackRatingAsync(string path, int rating)
        {
            await this.trackStatisticRepository.UpdateRatingAsync(path, rating);

            // Update the rating in the file if the user selected this option
            if (SettingsClient.Get<bool>("Behaviour", "SaveRatingToAudioFiles"))
            {
                // Only for MP3's
                if (Path.GetExtension(path).ToLower().Equals(FileFormats.MP3))
                {
                    IFileMetadata fmd = await this.GetFileMetadataAsync(path);
                    fmd.Rating = new MetadataRatingValue() { Value = rating };
                    await this.QueueUpdateFileMetadata(new IFileMetadata[] { fmd }.ToList());
                }
            }

            this.RatingChanged(new RatingChangedEventArgs { Path = path, Rating = rating });
        }

        public async Task UpdateTrackLoveAsync(string path, bool love)
        {
            await this.trackStatisticRepository.UpdateLoveAsync(path, love);

            this.LoveChanged(new LoveChangedEventArgs { Path = path, Love = love });
        }

        public async Task UpdateTracksAsync(List<IFileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            // Make sure that cached artwork cannot be out of date
            lock (this.cachedArtworkLock)
            {
                this.cachedArtwork = null;
            }

            // Set event args
            var args = new MetadataChangedEventArgs();

            foreach (IFileMetadata fmd in fileMetadatas)
            {
                if (fmd.Artists.IsValueChanged) args.IsArtistChanged = true;
                if (fmd.Genres.IsValueChanged) args.IsGenreChanged = true;
                if (fmd.Album.IsValueChanged || fmd.AlbumArtists.IsValueChanged || fmd.Year.IsValueChanged) args.IsAlbumChanged = true;
                if (fmd.ArtworkData.IsValueChanged) args.IsArtworkChanged = true;
                if (fmd.Title.IsValueChanged || fmd.Year.IsValueChanged || fmd.TrackNumber.IsValueChanged ||
                    fmd.TrackCount.IsValueChanged || fmd.DiscNumber.IsValueChanged || fmd.DiscCount.IsValueChanged ||
                    fmd.Lyrics.IsValueChanged) args.IsTrackChanged = true;
            }

            // Update the metadata in the database
            await this.UpdateDatabaseMetadataAsync(fileMetadatas, updateAlbumArtwork);

            // Queue update of the file metadata
            await this.QueueUpdateFileMetadata(fileMetadatas);

            // Update the metadata in the PlaybackService
            await this.playbackService.UpdateQueueMetadataAsync(fileMetadatas);

            // Raise event
            this.MetadataChanged(args);
        }

        public async Task UpdateAlbumAsync(Album album, MetadataArtworkValue artwork, bool updateFileArtwork)
        {
            // Make sure that cached artwork cannot be out of date
            lock (this.cachedArtworkLock)
            {
                this.cachedArtwork = null;
            }

            // Set event args
            var args = new MetadataChangedEventArgs() { IsArtworkChanged = true };

            // Cache new artwork
            string artworkID = await this.cacheService.CacheArtworkAsync(artwork.Value);

            // Update artwork in database
            await this.albumRepository.UpdateAlbumArtworkAsync(album.AlbumTitle, album.AlbumArtist, artworkID);

            List<PlayableTrack> albumTracks = await this.trackRepository.GetAlbumTracksAsync(new List<long> { album.AlbumID });

            var fileMetadatas = new List<IFileMetadata>();

            foreach (PlayableTrack track in albumTracks)
            {
                var fmd = await this.GetFileMetadataAsync(track.Path);
                fmd.ArtworkData = artwork;
                fileMetadatas.Add(fmd);
            }

            if (updateFileArtwork)
            {
                // Queue update of the file metadata
                await this.QueueUpdateFileMetadata(fileMetadatas);
            }

            // Update the metadata in the PlaybackService
            await this.playbackService.UpdateQueueMetadataAsync(fileMetadatas);

            // Raise event
            this.MetadataChanged(args);
        }

        public async Task<byte[]> GetArtworkAsync(string path)
        {
            byte[] artwork = null;

            await Task.Run(() =>
            {
                lock (this.cachedArtworkLock)
                {
                    // First, check if artwork for this path has been asked recently.
                    if (this.cachedArtwork != null && this.cachedArtwork.Item1.ToSafePath() == path.ToSafePath())
                    {
                        if (this.cachedArtwork.Item2 != null) artwork = this.cachedArtwork.Item2;
                    }

                    if (artwork == null)
                    {
                        // If no cached artwork was found, try to load embedded artwork.
                        IFileMetadata fmd = this.GetFileMetadata(path);

                        if (fmd.ArtworkData.Value != null)
                        {
                            artwork = fmd.ArtworkData.Value;
                            this.cachedArtwork = new Tuple<string, byte[]>(path, artwork);
                        }
                    }

                    if (artwork == null)
                    {
                        // If no embedded artwork was found, try to find external artwork.
                        artwork = IndexerUtils.GetExternalArtwork(path);
                        if (artwork != null) this.cachedArtwork = new Tuple<string, byte[]>(path, artwork);
                    }

                    if (artwork == null)
                    {
                        // If no embedded artwork was found, try to find album artwork.
                        Track track = this.trackRepository.GetTrack(path);
                        Album album = track != null ? this.albumRepository.GetAlbum(track.AlbumID) : null;

                        if (album != null)
                        {
                            string artworkPath = this.cacheService.GetCachedArtworkPath((string)album.ArtworkID);
                            if (!string.IsNullOrEmpty(artworkPath)) artwork = ImageUtils.Image2ByteArray(artworkPath);
                            if (artwork != null) this.cachedArtwork = new Tuple<string, byte[]>(path, artwork);
                        }
                    }
                }
            });

            return artwork;
        }

        public async Task<byte[]> GetArtworkScaledAsync(string path, int width, int height)
        {
            byte[] scaledArtwork = null;

            try
            {
                byte[] artwork = await this.GetArtworkAsync(path);

                if(artwork != null)
                {
                    await Task.Run(() => scaledArtwork = ImageUtils.ResizeImageInByteArray(artwork, width, height));
                }
            }
            catch (Exception)
            {
                // Intended suppression
            }

            return scaledArtwork;
        }

        public async Task SafeUpdateFileMetadataAsync()
        {
            if (this.isUpdatingFileMetadata)
            {
                while (this.isUpdatingFileMetadata)
                {
                    await Task.Delay(50);
                }
            }

            // In case the previous loop didn't save all metadata to files, force it again.
            await this.UpdateFileMetadataAsync();
        }

        private async Task QueueUpdateFileMetadata(List<IFileMetadata> fileMetadatas)
        {
            this.updateFileMetadataTimer.Stop();

            await Task.Run(() =>
            {
                lock (this.lockObject)
                {
                    foreach (IFileMetadata fmd in fileMetadatas)
                    {
                        if (this.fileMetadataDictionary.ContainsKey(fmd.SafePath))
                        {
                            this.fileMetadataDictionary[fmd.SafePath] = fmd;
                        }
                        else
                        {
                            this.fileMetadataDictionary.Add(fmd.SafePath, fmd);
                        }
                    }
                }
            });

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataShortTimeout; // The next time, almost don't wait.
            this.updateFileMetadataTimer.Start();
        }

        private async Task UpdateFileMetadataAsync()
        {
            bool restartTimer = false;
            this.updateFileMetadataTimer.Stop();

            this.isUpdatingFileMetadata = true;

            var filesToSync = new List<IFileMetadata>();

            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    int numberToProcess = this.fileMetadataDictionary.Count;
                    if (numberToProcess == 0) return;

                    while (numberToProcess > 0)
                    {
                        IFileMetadata fmd = this.fileMetadataDictionary.First().Value;
                        numberToProcess--;

                        try
                        {
                            fmd.Save();
                            filesToSync.Add(fmd);
                            this.fileMetadataDictionary.Remove(fmd.SafePath);
                        }
                        catch (IOException ex)
                        {
                            LogClient.Error("Unable to save metadata to the file for Track '{0}'. The file is probably playing. Trying again in {1} seconds. Exception: {2}", fmd.SafePath, this.updateFileMetadataLongTimeout / 1000, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Unable to save metadata to the file for Track '{0}'. Not trying again. Exception: {1}", fmd.SafePath, ex.Message);
                            this.fileMetadataDictionary.Remove(fmd.SafePath);
                        }
                    }

                    // If there are still queued FileMetadata's, start the timer.
                    if (this.fileMetadataDictionary.Count > 0) restartTimer = true;
                }
            });

            // Sync file size and last modified date in the database
            foreach (IFileMetadata fmd in filesToSync)
            {
                await this.trackRepository.UpdateTrackFileInformationAsync(fmd.SafePath);
            }

            this.updateFileMetadataTimer.Interval = this.updateFileMetadataLongTimeout; // The next time, wait longer.

            this.isUpdatingFileMetadata = false;

            if (restartTimer) this.updateFileMetadataTimer.Start();
        }

        private async Task UpdateDatabaseMetadataAsync(IFileMetadata fileMetadata, bool updateAlbumArtwork)
        {
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);
            if (track == null) return;

            // Track
            if (fileMetadata.Title.IsValueChanged) track.TrackTitle = fileMetadata.Title.Value;
            if (fileMetadata.Year.IsValueChanged) track.Year = fileMetadata.Year.Value.SafeConvertToLong();
            if (fileMetadata.TrackNumber.IsValueChanged) track.TrackNumber = fileMetadata.TrackNumber.Value.SafeConvertToLong();
            if (fileMetadata.TrackCount.IsValueChanged) track.TrackCount = fileMetadata.TrackCount.Value.SafeConvertToLong();
            if (fileMetadata.DiscNumber.IsValueChanged) track.DiscNumber = fileMetadata.DiscNumber.Value.SafeConvertToLong();
            if (fileMetadata.DiscCount.IsValueChanged) track.DiscCount = fileMetadata.DiscCount.Value.SafeConvertToLong();
            if (fileMetadata.Lyrics.IsValueChanged) track.HasLyrics = string.IsNullOrWhiteSpace(fileMetadata.Lyrics.Value) ? 0 : 1;

            // Artist
            if (fileMetadata.Artists.IsValueChanged)
            {
                string newArtistName = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownArtistText;
                Artist artist = await this.artistRepository.GetArtistAsync(newArtistName);
                if (artist == null) artist = await this.artistRepository.AddArtistAsync(new Artist { ArtistName = newArtistName });
                if (artist != null) track.ArtistID = artist.ArtistID;
            }

            // Genre
            if (fileMetadata.Genres.IsValueChanged)
            {
                string newGenreName = fileMetadata.Genres.Values != null && !string.IsNullOrEmpty(fileMetadata.Genres.Values.FirstOrDefault()) ? fileMetadata.Genres.Values.FirstOrDefault() : Defaults.UnknownGenreText;
                Genre genre = await this.genreRepository.GetGenreAsync(newGenreName);
                if (genre == null) genre = await this.genreRepository.AddGenreAsync(new Genre { GenreName = newGenreName });
                if (genre != null) track.GenreID = genre.GenreID;
            }

            // Album
            if (fileMetadata.Album.IsValueChanged || fileMetadata.AlbumArtists.IsValueChanged || fileMetadata.Year.IsValueChanged)
            {
                string newAlbumTitle = !string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumText;
                string newAlbumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : Defaults.UnknownArtistText;
                Album album = await this.albumRepository.GetAlbumAsync(newAlbumTitle, newAlbumArtist);

                if (album == null)
                {
                    album = new Album { AlbumTitle = newAlbumTitle, AlbumArtist = newAlbumArtist, DateLastSynced = DateTime.Now.Ticks };
                    album.ArtworkID = await this.cacheService.CacheArtworkAsync(IndexerUtils.GetArtwork(album, this.metadataFactory.Create(track.Path)));
                    album = await this.albumRepository.AddAlbumAsync(album);
                }

                if (album != null) track.AlbumID = album.AlbumID;

                await Task.Run(() => MetadataUtils.UpdateAlbumYear(album, fileMetadata.Year.Value.SafeConvertToLong())); // Update Album year
                await this.albumRepository.UpdateAlbumAsync(album);
            }

            await this.trackRepository.UpdateTrackAsync(track); // Update Track in the database

            if (updateAlbumArtwork)
            {
                // Get album artist
                string albumArtist = fileMetadata.AlbumArtists.Values != null && !string.IsNullOrEmpty(fileMetadata.AlbumArtists.Values.FirstOrDefault()) ? fileMetadata.AlbumArtists.Values.FirstOrDefault() : string.Empty;

                // If no album artist is found, use the artist name. The album was probably saved using the artist name.
                if (string.IsNullOrEmpty(albumArtist))
                {
                    albumArtist = fileMetadata.Artists.Values != null && !string.IsNullOrEmpty(fileMetadata.Artists.Values.FirstOrDefault()) ? fileMetadata.Artists.Values.FirstOrDefault() : Defaults.UnknownArtistText;
                }

                // Get the album title
                string albumTitle = !string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? fileMetadata.Album.Value : Defaults.UnknownAlbumText;

                // Cache the new artwork
                string artworkID = await this.cacheService.CacheArtworkAsync(fileMetadata.ArtworkData.Value);

                // Update the album artwork in the database
                await this.albumRepository.UpdateAlbumArtworkAsync(albumTitle, albumArtist, artworkID);
            }
        }

        private async Task UpdateDatabaseMetadataAsync(List<IFileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            this.isUpdatingDatabaseMetadata = true;

            foreach (IFileMetadata fmd in fileMetadatas)
            {
                try
                {
                    await this.UpdateDatabaseMetadataAsync(fmd, updateAlbumArtwork);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Unable to update database metadata for Track '{0}'. Exception: {1}", fmd.SafePath, ex.Message);
                }
            }

            try
            {
                await this.albumRepository.DeleteOrphanedAlbumsAsync();
                await this.genreRepository.DeleteOrphanedGenresAsync();
                await this.artistRepository.DeleteOrphanedArtistsAsync();
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while deleting orphans. Exception: {0}", ex.Message);
            }

            this.isUpdatingDatabaseMetadata = false;
        }
    }
}
