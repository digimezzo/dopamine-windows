using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Extensions;
using Dopamine.Common.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playback
{
    public class QueueManager
    {
        #region Variables
        private PlayableTrack currentTrack;
        private object queueLock = new object();
        private List<PlayableTrack> queuedTracks = new List<PlayableTrack>(); // Queued Tracks in original order
        private List<PlayableTrack> shuffledTracks = new List<PlayableTrack>(); // Queued Tracks in original order or shuffled
        #endregion

        #region Properties
        public List<PlayableTrack> Queue
        {
            get { return this.queuedTracks; }
        }
        #endregion

        #region Private
        private bool UpdateTrackPlaybackInfo(PlayableTrack track, FileMetadata fileMetadata)
        {
            bool isPlaybackInfoUpdated = false;

            try
            {
                // Only update the properties that are displayed on Now Playing screens
                if (fileMetadata.Title.IsValueChanged)
                {
                    track.TrackTitle = fileMetadata.Title.Value;
                    isPlaybackInfoUpdated = true;
                }

                if (fileMetadata.Artists.IsValueChanged)
                {
                    track.ArtistName = fileMetadata.Artists.Values.FirstOrDefault();
                    isPlaybackInfoUpdated = true;
                }

                if (fileMetadata.Year.IsValueChanged)
                {
                    track.Year = fileMetadata.Year.Value.SafeConvertToLong();
                    isPlaybackInfoUpdated = true;
                }

                if (fileMetadata.Album.IsValueChanged)
                {
                    track.AlbumTitle = fileMetadata.Album.Value;
                    isPlaybackInfoUpdated = true;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not update the track metadata. Exception: {0}", ex.Message);
            }

            return isPlaybackInfoUpdated;
        }
        #endregion

        #region public
        public async Task ShuffleAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queuedTracks.Count > 0)
                    {
                        // Make sure the lists are deep copies
                        this.shuffledTracks = new List<PlayableTrack>(this.queuedTracks).Randomize();
                    }
                }
            });
        }

        public async Task UnShuffleAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    // Make sure the lists are deep copies
                    this.shuffledTracks = new List<PlayableTrack>(this.queuedTracks);
                }
            });
        }

        public PlayableTrack CurrentTrack()
        {
            try
            {
                if (this.currentTrack != null)
                {
                    return this.currentTrack;
                }
                else if (this.shuffledTracks != null && this.shuffledTracks.Count > 0)
                {
                    return this.shuffledTracks.First();
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get current track. Exception: {0}", ex.Message);
            }

            return null;
        }

        public async Task<PlayableTrack> PreviousTrackAsync(LoopMode loopMode)
        {
            PlayableTrack previousTrack = null;

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.shuffledTracks != null && this.shuffledTracks.Count > 0)
                        {
                            int firstIndex = 0;
                            int lastIndex = this.shuffledTracks.Count - 1;
                            int currentTrackIndex = this.shuffledTracks.IndexOf(this.currentTrack);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                previousTrack = this.shuffledTracks[currentTrackIndex];
                            }
                            else
                            {
                                if (currentTrackIndex > firstIndex)
                                {
                                    // If we didn't reach the start of the queue, return the previous track.
                                    previousTrack = this.shuffledTracks[currentTrackIndex - 1];
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the start of the queue, return the last track.
                                    previousTrack = this.shuffledTracks[lastIndex];
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get previous track. Exception: {0}", ex.Message);
                }
            });

            return previousTrack;
        }

        public async Task<PlayableTrack> NextTrackAsync(LoopMode loopMode)
        {
            PlayableTrack nextTrack = null;

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.shuffledTracks != null && this.shuffledTracks.Count > 0)
                        {
                            int firstIndex = 0;
                            int lastIndex = this.shuffledTracks.Count - 1;
                            int playingTrackIndex = this.shuffledTracks.IndexOf(this.currentTrack);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                nextTrack = this.shuffledTracks[playingTrackIndex];
                            }
                            else
                            {
                                if (playingTrackIndex < lastIndex)
                                {
                                    // If we didn't reach the end of the queue, return the next track.
                                    nextTrack = this.shuffledTracks[playingTrackIndex + 1];
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the end of the queue, return the first track.
                                    nextTrack = this.shuffledTracks[firstIndex];
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get next track. Exception: {0}", ex.Message);
                }
            });

            return nextTrack;
        }

        public async Task<EnqueueResult> EnqueueAsync(IList<PlayableTrack> tracks)
        {
            var result = new EnqueueResult { IsSuccess = true };

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        // Only enqueue tracks which are not queued yet
                        result.EnqueuedTracks = tracks.Except(this.queuedTracks).ToList();
                        this.queuedTracks.AddRange(result.EnqueuedTracks);
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    LogClient.Error("Error while enqueuing tracks. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<EnqueueResult> EnqueueNextAsync(IList<PlayableTrack> tracks)
        {
            var result = new EnqueueResult { IsSuccess = true };

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        result.EnqueuedTracks = tracks;

                        int queuedIndex = 0;
                        int shuffledIndex = 0;

                        if (this.currentTrack != null)
                        {
                            queuedIndex = this.queuedTracks.IndexOf(this.currentTrack);
                            shuffledIndex = this.shuffledTracks.IndexOf(this.currentTrack);
                        }

                        this.queuedTracks.InsertRange(queuedIndex + 1, tracks);
                        this.shuffledTracks.InsertRange(shuffledIndex + 1, tracks);
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    LogClient.Error("Error while enqueuing tracks next. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<bool> ClearQueueAsync()
        {
            bool isSuccess = true;

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        this.queuedTracks.Clear();
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    LogClient.Error("Error while clearing queue. Exception: {0}", ex.Message);
                }
            });

            return isSuccess;
        }

        public async Task<bool> IsQueueDifferentAsync(IList<PlayableTrack> tracks)
        {
            bool isQueueDifferent = false;

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queuedTracks == null || this.queuedTracks.Count == 0 || tracks.Count != this.queuedTracks.Count)
                    {
                        isQueueDifferent = true;
                    }
                    else if (this.queuedTracks.Except(tracks).ToList().Count != 0)
                    {
                        isQueueDifferent = true;
                    }
                }
            });

            return isQueueDifferent;
        }

        public async Task<DequeueResult> DequeueAsync(IList<PlayableTrack> tracks)
        {
            bool isSuccess = true;
            var removedQueuedTracks = new List<PlayableTrack>();
            var removedShuffledTracks = new List<PlayableTrack>();
            int indexOfLastRemovedQueuedTrack = 0;
            bool isPlayingTrackDequeued = false;

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    foreach (PlayableTrack t in tracks)
                    {
                        try
                        {
                            // Remove from this.queuedTracks. The index doesn't matter.
                            if (this.queuedTracks.Contains(t))
                            {
                                this.queuedTracks.Remove(t);
                                removedQueuedTracks.Add(t);
                            }
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Error("Error while removing queued track with path='{0}'. Exception: {1}", t.Path, ex.Message);
                        }
                    }

                    foreach (PlayableTrack removedQueuedTrack in removedQueuedTracks)
                    {
                        // Remove from this.shuffledTracks. The index does matter,
                        // as we might have to play the next available Track.
                        try
                        {
                            int indexOfCurrentRemovedQueuedTrack = this.shuffledTracks.IndexOf(removedQueuedTrack);

                            if (indexOfCurrentRemovedQueuedTrack >= 0)
                            {
                                if (this.shuffledTracks[indexOfCurrentRemovedQueuedTrack].Equals(this.currentTrack))
                                {
                                    isPlayingTrackDequeued = true;
                                }

                                PlayableTrack removedShuffledTrack = this.shuffledTracks[indexOfCurrentRemovedQueuedTrack];
                                this.shuffledTracks.RemoveAt(indexOfCurrentRemovedQueuedTrack);
                                removedShuffledTracks.Add(removedShuffledTrack);
                                if (indexOfCurrentRemovedQueuedTrack < indexOfLastRemovedQueuedTrack) indexOfLastRemovedQueuedTrack = indexOfCurrentRemovedQueuedTrack;
                            }
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Error("Error while removing shuffled track with path='{0}'. Exception: {1}", removedQueuedTrack.Path, ex.Message);
                        }
                    }
                }
            });

            var dequeueResult = new DequeueResult { IsSuccess = isSuccess, DequeuedTracks = removedShuffledTracks, IsPlayingTrackDequeued = isPlayingTrackDequeued };

            if (isSuccess & isPlayingTrackDequeued & this.shuffledTracks.Count > indexOfLastRemovedQueuedTrack)
            {
                dequeueResult.NextAvailableTrack = this.shuffledTracks[indexOfLastRemovedQueuedTrack];
            }

            return dequeueResult;
        }

        public void SetCurrentTrack(PlayableTrack track)
        {
            this.currentTrack = track;
        }

        public async Task<bool> UpdateQueueOrderAsync(List<PlayableTrack> tracks, bool isShuffled)
        {
            if (tracks == null || tracks.Count == 0) return false;

            bool isSuccess = true;

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        this.queuedTracks = new List<PlayableTrack>(tracks);

                        if (!isShuffled)
                        {
                            this.shuffledTracks = new List<PlayableTrack>(this.queuedTracks);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                isSuccess = false;
                LogClient.Error("Could update queue order. Exception: {0}", ex.Message);
            }

            return isSuccess;
        }

        public async Task<UpdateQueueMetadataResult> UpdateQueueMetadataAsync(List<FileMetadata> fileMetadatas)
        {
            var result = new UpdateQueueMetadataResult();

            await Task.Run(() =>
                {
                    // Update playing track
                    if (this.currentTrack != null)
                    {
                        FileMetadata fmd = fileMetadatas.Select(f => f).Where(f => f.SafePath == this.currentTrack.SafePath).FirstOrDefault();

                        if (fmd != null)
                        {
                            result.IsPlayingTrackPlaybackInfoChanged = this.UpdateTrackPlaybackInfo(this.currentTrack, fmd);
                            result.IsPlayingTrackArtworkChanged = fmd.ArtworkData.IsValueChanged;
                        }
                    }

                    // Update queue
                    lock (this.queueLock)
                    {
                        if (this.Queue != null)
                        {
                            bool isQueueChanged = false;

                            foreach (PlayableTrack track in this.queuedTracks)
                            {
                                FileMetadata fmd = fileMetadatas.Select(f => f).Where(f => f.SafePath == track.SafePath).FirstOrDefault();

                                if (fmd != null)
                                {
                                    this.UpdateTrackPlaybackInfo(track, fmd);
                                    isQueueChanged = true;
                                }
                            }

                            foreach (PlayableTrack track in this.shuffledTracks)
                            {
                                FileMetadata fmd = fileMetadatas.Select(f => f).Where(f => f.SafePath == track.SafePath).FirstOrDefault();

                                if (fmd != null)
                                {
                                    this.UpdateTrackPlaybackInfo(track, fmd);
                                    isQueueChanged = true;
                                }
                            }

                            if (isQueueChanged) result.IsQueueChanged = true;
                        }
                    }
                });

            return result;
        }
        #endregion
    }
}
