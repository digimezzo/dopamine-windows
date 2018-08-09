using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Indexing;
using Dopamine.Services.Playback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Metadata
{
    public class MetadataService : IMetadataService
    {
        private IPlaybackService playbackService;
        private ICacheService cacheService;
        private ITrackRepository trackRepository;
        private IAlbumArtworkRepository albumArtworkRepository;
        private IFileMetadataFactory metadataFactory;
        private FileMetadataUpdater updater;
        private Tuple<string, byte[]> cachedArtwork;
        private object cachedArtworkLock = new object();

        public event Action<MetadataChangedEventArgs> MetadataChanged = delegate { };
        public event Action<RatingChangedEventArgs> RatingChanged = delegate { };
        public event Action<LoveChangedEventArgs> LoveChanged = delegate { };

        public MetadataService(IPlaybackService playbackService, ICacheService cacheService, ITrackRepository trackRepository,
            IAlbumArtworkRepository albumArtworkRepository, IFileMetadataFactory metadataFactory)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.trackRepository = trackRepository;
            this.albumArtworkRepository = albumArtworkRepository;
            this.metadataFactory = metadataFactory;

            this.updater = new FileMetadataUpdater(this.playbackService, this.trackRepository);
        }

        public IFileMetadata GetFileMetadata(string path)
        {
            // First, check if there is a fileMetadata which is queued for saving.
            // If yes, use that, as it has more up to date information.
            IFileMetadata fileMetadata = this.updater.GetFileMetadataToUpdate(path);

            if (fileMetadata == null)
            {
                // If not, create a new fileMetadata from the file path.
                fileMetadata = this.metadataFactory.Create(path);
            }

            return fileMetadata;
        }

        public async Task<IFileMetadata> GetFileMetadataAsync(string path)
        {
            IFileMetadata fileMetadata = null;

            await Task.Run(() => fileMetadata = this.GetFileMetadata(path));

            return fileMetadata;
        }

        public async Task UpdateTrackRatingAsync(string path, int rating)
        {
            await this.trackRepository.UpdateRatingAsync(path, rating);

            // Update the rating in the file if the user selected this option
            if (SettingsClient.Get<bool>("Behaviour", "SaveRatingToAudioFiles"))
            {
                // Only for MP3's
                if (Path.GetExtension(path).ToLower().Equals(FileFormats.MP3))
                {
                    IFileMetadata fmd = await this.GetFileMetadataAsync(path);
                    fmd.Rating = new MetadataRatingValue() { Value = rating };
                    await this.updater.UpdateFileMetadataAsync(new IFileMetadata[] { fmd }.ToList());
                }
            }

            this.RatingChanged(new RatingChangedEventArgs(path.ToSafePath(), rating));
        }

        public async Task UpdateTrackLoveAsync(string path, bool love)
        {
            await this.trackRepository.UpdateLoveAsync(path, love ? 1 : 0);

            this.LoveChanged(new LoveChangedEventArgs(path.ToSafePath(), love));
        }

        private byte[] GetCachedArtwork(string filename)
        {
            byte[] artwork = null;

            if (this.cachedArtwork != null && this.cachedArtwork.Item1.ToSafePath() == filename.ToSafePath())
            {
                if (this.cachedArtwork.Item2 != null) artwork = this.cachedArtwork.Item2;
            }

            return artwork;
        }

        private byte[] GetEmbeddedArtwork(string filename, int size)
        {
            byte[] artwork = null;

            IFileMetadata fmd = this.GetFileMetadata(filename);

            if (fmd.ArtworkData.Value != null)
            {
                // If size > 0, resize the artwork. Otherwise, get the full artwork.
                artwork = size > 0 ? ImageUtils.ResizeImageInByteArray(fmd.ArtworkData.Value, size, size) : fmd.ArtworkData.Value;
            }

            return artwork;
        }

        private byte[] GetExternalArtwork(string filename, int size)
        {
            byte[] artwork = null;

            artwork = IndexerUtils.GetExternalArtwork(filename, size, size);

            if (artwork != null)
            {
                this.cachedArtwork = new Tuple<string, byte[]>(filename, artwork);
            }

            return artwork;
        }

        private byte[] GetAlbumArtwork(string filename, int size)
        {
            byte[] artwork = null;

            Task<AlbumArtwork> task = this.albumArtworkRepository.GetAlbumArtworkForPathAsync(filename);
            AlbumArtwork albumArtwork = task.Result;

            if (albumArtwork != null)
            {
                string artworkPath = this.cacheService.GetCachedArtworkPath(albumArtwork.ArtworkID);

                if (!string.IsNullOrEmpty(artworkPath))
                {
                    artwork = ImageUtils.Image2ByteArray(artworkPath, size, size);
                }

                if (artwork != null)
                {
                    this.cachedArtwork = new Tuple<string, byte[]>(filename, artwork);
                }
            }

            return artwork;
        }

        public async Task<byte[]> GetArtworkAsync(string filename, int size = 0)
        {
            byte[] artwork = null;

            await Task.Run(() =>
            {
                lock (this.cachedArtworkLock)
                {
                    // First, if we need full size artwork, check if artwork for this path has been asked for recently.
                    if (size == 0)
                    {
                        artwork = this.GetCachedArtwork(filename);
                    }

                    if (artwork == null)
                    {
                        // If no cached artwork was found, try to find embedded artwork.
                        artwork = this.GetEmbeddedArtwork(filename, size);
                    }

                    if (artwork == null)
                    {
                        // If no embedded artwork was found, try to find external artwork.
                        artwork = this.GetExternalArtwork(filename, size);
                    }

                    if (artwork == null)
                    {
                        // If no external artwork was found, try to find album artwork.
                        artwork = this.GetAlbumArtwork(filename, size);
                    }

                    if (artwork != null && size == 0)
                    {
                        // If any artwork was found, and it wasn't resized, cache it for the next request for this file.
                        this.cachedArtwork = new Tuple<string, byte[]>(filename, artwork);
                    }
                }
            });

            return artwork;
        }

        public async Task ForceSaveFileMetadataAsync()
        {
            await this.updater.ForceUpdateFileMetadataAsync();
        }

        private async Task UpdateDatabaseMetadataAsync(IFileMetadata fileMetadata, bool updateAlbumArtwork)
        {
            // Get the track from the database
            Track track = await this.trackRepository.GetTrackAsync(fileMetadata.SafePath);

            if (track == null)
            {
                return;
            }

            // Update track fields
            await Task.Run(() => MetadataUtils.FillTrackBase(fileMetadata, ref track));

            // Update the Track in the database
            await this.trackRepository.UpdateTrackAsync(track);

            if (updateAlbumArtwork)
            {
                // Cache the new artwork
                string artworkID = await this.cacheService.CacheArtworkAsync(fileMetadata.ArtworkData.Value);

                // Add or update AlbumArtwork in the database
                await this.albumArtworkRepository.UpdateAlbumArtworkAsync(track.AlbumKey, artworkID);
            }
        }

        private async Task UpdateDatabaseMetadataAsync(IList<IFileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            foreach (IFileMetadata fileMetadata in fileMetadatas)
            {
                try
                {
                    await this.UpdateDatabaseMetadataAsync(fileMetadata, updateAlbumArtwork);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Unable to update database metadata for Track '{0}'. Exception: {1}", fileMetadata.SafePath, ex.Message);
                }
            }
        }

        public async Task UpdateTracksAsync(IList<IFileMetadata> fileMetadatas, bool updateAlbumArtwork)
        {
            // Make sure that cached artwork cannot be out of date
            lock (this.cachedArtworkLock)
            {
                this.cachedArtwork = null;
            }

            // Update metadata in the files
            await this.updater.UpdateFileMetadataAsync(fileMetadatas);

            // Update metadata in the database
            await this.UpdateDatabaseMetadataAsync(fileMetadatas, updateAlbumArtwork);

            // Update metadata in the PlaybackService queue
            await this.playbackService.UpdateQueueMetadataAsync(fileMetadatas);

            // Raise event
            this.MetadataChanged(new MetadataChangedEventArgs());
        }

        public async Task UpdateAlbumAsync(AlbumViewModel albumViewModel, MetadataArtworkValue artwork, bool updateFileArtwork)
        {
            // Make sure that cached artwork cannot be out of date
            lock (this.cachedArtworkLock)
            {
                this.cachedArtwork = null;
            }

            // Cache the new artwork
            string artworkID = await this.cacheService.CacheArtworkAsync(artwork.Value);

            // Add or update AlbumArtwork in the database
            await this.albumArtworkRepository.UpdateAlbumArtworkAsync(albumViewModel.AlbumKey, artworkID);

            if (updateFileArtwork)
            {
                // Get the tracks for this album
                IList<Track> tracks = await this.trackRepository.GetAlbumTracksAsync(new List<string> { albumViewModel.AlbumKey });
                IList<IFileMetadata> fileMetadatas = new List<IFileMetadata>();

                foreach (Track track in tracks)
                {
                    IFileMetadata fileMetadata = await this.GetFileMetadataAsync(track.Path);
                    fileMetadata.ArtworkData = artwork;
                    fileMetadatas.Add(fileMetadata);
                }

                // Update metadata in the files
                await this.updater.UpdateFileMetadataAsync(fileMetadatas);
            }

            // Raise event
            this.MetadataChanged(new MetadataChangedEventArgs());
        }
    }
}
