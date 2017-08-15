using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Extensions;
using Dopamine.Common.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dopamine.Common.Helpers;

namespace Dopamine.Common.Services.Playback
{
    internal class QueueManager
    {
        #region Variables
        private KeyValuePair<string, PlayableTrack> currentTrack;
        private object queueLock = new object();
        private OrderedDictionary<string, PlayableTrack> queue = new OrderedDictionary<string, PlayableTrack>(); // Queued tracks in original order
        private List<string> playbackOrder = new List<string>(); // Playback order of queued tracks
        #endregion

        #region Properties
        public OrderedDictionary<string, PlayableTrack> Queue
        {
            get { return this.queue; }
        }
        #endregion

        #region Private
        private bool UpdateTrackPlaybackInfo(PlayableTrack track, FileMetadata fileMetadata)
        {
            bool isDisplayedPlaybackInfoChanged = false;

            try
            {
                // Only update the properties that are displayed on Now Playing screens
                if (fileMetadata.Title.IsValueChanged)
                {
                    track.TrackTitle = fileMetadata.Title.Value;
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Artists.IsValueChanged)
                {
                    track.ArtistName = fileMetadata.Artists.Values.FirstOrDefault();
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Year.IsValueChanged)
                {
                    track.Year = fileMetadata.Year.Value.SafeConvertToLong();
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Album.IsValueChanged)
                {
                    track.AlbumTitle = fileMetadata.Album.Value;
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.ArtworkData.IsValueChanged)
                {
                    isDisplayedPlaybackInfoChanged = true;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not update the track metadata. Exception: {0}", ex.Message);
            }

            return isDisplayedPlaybackInfoChanged;
        }
        #endregion

        #region public
        public async Task ShuffleAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queue.Count > 0)
                    {
                        if (this.currentTrack.Equals(default(KeyValuePair<string, PlayableTrack>)) || !this.queue.Keys.Contains(this.currentTrack.Key))
                        {
                            // We're not playing a track from the queue: just shuffle.
                            this.playbackOrder = new List<string>(this.queue.Keys).Randomize();
                        }
                        else
                        {
                            // We're playing a track from the queue: shuffle, but make sure the playing track comes first.
                            this.playbackOrder = new List<string>();
                            this.playbackOrder.Add(this.currentTrack.Key);
                            List<string> queueCopy = new List<string>(this.queue.Keys);
                            queueCopy.Remove(this.currentTrack.Key);
                            this.playbackOrder.AddRange(new List<string>(queueCopy).Randomize());
                        }
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
                    if (this.queue.Count > 0)
                    {
                        this.playbackOrder = new List<string>(this.queue.Keys);
                    }
                }
            });
        }

        public KeyValuePair<string, PlayableTrack> CurrentTrack()
        {
            try
            {
                if (!this.currentTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
                {
                    return this.currentTrack;
                }
                else
                {
                    return this.FirstTrack();
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get current track. Exception: {0}", ex.Message);
            }

            return default(KeyValuePair<string, PlayableTrack>);
        }

        public KeyValuePair<string, PlayableTrack> FirstTrack()
        {
            KeyValuePair<string, PlayableTrack> firstTrack = default(KeyValuePair<string, PlayableTrack>);

            try
            {
                if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                {
                    firstTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder.First(), this.queue[this.playbackOrder.First()]);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get first track. Exception: {0}", ex.Message);
            }

            return firstTrack;
        }

        public async Task<KeyValuePair<string, PlayableTrack>> PreviousTrackAsync(LoopMode loopMode)
        {
            KeyValuePair<string, PlayableTrack> previousTrack = default(KeyValuePair<string, PlayableTrack>);

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.playbackOrder.IndexOf(this.currentTrack.Key);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                previousTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder[currentTrackIndex], this.queue[this.playbackOrder[currentTrackIndex]]);
                            }
                            else
                            {
                                if (currentTrackIndex > 0)
                                {
                                    // If we didn't reach the start of the queue, return the previous track.
                                    previousTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder[currentTrackIndex - 1], this.queue[this.playbackOrder[currentTrackIndex - 1]]);
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the start of the queue, return the last track.
                                    previousTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder.Last(), this.queue[this.playbackOrder.Last()]);
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

        public async Task<KeyValuePair<string, PlayableTrack>> NextTrackAsync(LoopMode loopMode)
        {
            KeyValuePair<string, PlayableTrack> nextTrack = default(KeyValuePair<string, PlayableTrack>);

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.playbackOrder.IndexOf(this.currentTrack.Key);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                nextTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder[currentTrackIndex], this.queue[this.playbackOrder[currentTrackIndex]]);
                            }
                            else
                            {
                                if (currentTrackIndex < this.playbackOrder.Count - 1)
                                {
                                    // If we didn't reach the end of the queue, return the next track.
                                    nextTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder[currentTrackIndex + 1], this.queue[this.playbackOrder[currentTrackIndex + 1]]);
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the end of the queue, return the first track.
                                    nextTrack = new KeyValuePair<string, PlayableTrack>(this.playbackOrder.First(), this.queue[this.playbackOrder.First()]);
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

        public async Task<EnqueueResult> EnqueueAsync(IList<PlayableTrack> tracks, bool shuffle)
        {
            var result = new EnqueueResult { IsSuccess = true };

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        foreach (PlayableTrack track in tracks)
                        {
                            this.queue.Add(Guid.NewGuid().ToString(), track);
                        }

                        result.EnqueuedTracks = tracks;
                    }
                });

                if (shuffle)
                    await this.ShuffleAsync();
                else
                    await this.UnShuffleAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                LogClient.Error("Error while enqueuing tracks. Exception: {0}", ex.Message);
            }


            return result;
        }

        public async Task<EnqueueResult> EnqueueAsync(IList<KeyValuePair<string, PlayableTrack>> tracks, bool shuffle)
        {
            var result = new EnqueueResult { IsSuccess = true };

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        foreach (KeyValuePair<string, PlayableTrack> track in tracks)
                        {
                            this.queue.Add(track.Key, track.Value);
                        }

                        result.EnqueuedTracks = tracks.Select(t => t.Value).ToList();
                    }
                });

                if (shuffle)
                    await this.ShuffleAsync();
                else
                    await this.UnShuffleAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                LogClient.Error("Error while enqueuing tracks. Exception: {0}", ex.Message);
            }

            return result;
        }

        public async Task<EnqueueResult> EnqueueNextAsync(IList<PlayableTrack> tracks)
        {
            var result = new EnqueueResult { IsSuccess = true };

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        int queueIndex = 0;
                        int playbackOrderIndex = 0;

                        if (!this.currentTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
                        {
                            queueIndex = this.queue.IndexOf(this.currentTrack.Key);
                            playbackOrderIndex = this.playbackOrder.IndexOf(this.currentTrack.Key);
                        }

                        var kvp = new List<KeyValuePair<string, PlayableTrack>>();

                        foreach (PlayableTrack track in tracks)
                        {
                            kvp.Add(new KeyValuePair<string, PlayableTrack>(Guid.NewGuid().ToString(), track));
                        }

                        this.queue.InsertRange(queueIndex + 1, kvp);
                        this.playbackOrder.InsertRange(playbackOrderIndex + 1, kvp.Select(t => t.Key));

                        result.EnqueuedTracks = tracks;
                    }
                });
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                LogClient.Error("Error while enqueuing tracks next. Exception: {0}", ex.Message);
            }


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
                        this.currentTrack = default(KeyValuePair<string, PlayableTrack>);
                        this.queue.Clear();
                        this.playbackOrder.Clear();
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
                    if (this.queue == null || this.queue.Count == 0 || tracks.Count != this.queue.Count)
                    {
                        isQueueDifferent = true;
                    }
                    else if (this.queue.Values.Except(tracks).ToList().Count != 0)
                    {
                        isQueueDifferent = true;
                    }
                }
            });

            return isQueueDifferent;
        }

        public async Task<bool> IsQueueDifferentAsync(IList<KeyValuePair<string, PlayableTrack>> tracks)
        {
            bool isQueueDifferent = false;

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queue == null || this.queue.Count == 0 || tracks.Count != this.queue.Count)
                    {
                        isQueueDifferent = true;
                    }
                    else
                    {
                        foreach (KeyValuePair<string, PlayableTrack> track in tracks)
                        {
                            if (!this.queue.ContainsKey(track.Key))
                            {
                                isQueueDifferent = true;
                                break;
                            }
                        }
                    }
                }
            });

            return isQueueDifferent;
        }

        public async Task<DequeueResult> DequeueAsync(IList<KeyValuePair<string, PlayableTrack>> tracks)
        {
            bool isSuccess = true;
            var dequeuedTracks = new List<KeyValuePair<string, PlayableTrack>>();
            int indexOfLastDeueuedTrack = 0;
            bool isPlayingTrackDequeued = false;

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    foreach (var track in tracks)
                    {
                        try
                        {
                            if (this.queue.ContainsKey(track.Key))
                            {
                                // If the key is known, remove by key.
                                this.queue.Remove(track.Key);
                                dequeuedTracks.Add(track);

                                // If the key is known, indicate if the current track was dequeued by comparing the keys.
                                isPlayingTrackDequeued = isPlayingTrackDequeued || track.Key.Equals(this.currentTrack.Key);
                            }
                            else
                            {
                                // If the key is not known, get all queued tracks which have the same path.
                                var queuedTracksWithSamePath = this.queue.Select(t => t).Where(t => t.Value.SafePath.Equals(track.Value.SafePath)).ToList();

                                // Remove all queued track which have the same path
                                foreach (var queuedTrackWithSamePath in queuedTracksWithSamePath)
                                {
                                    this.queue.Remove(queuedTrackWithSamePath.Key);
                                    dequeuedTracks.Add(queuedTrackWithSamePath);

                                    // If the key is not known, indicate if the current track was dequeued by comparing the paths.
                                    isPlayingTrackDequeued = isPlayingTrackDequeued || queuedTrackWithSamePath.Value.Equals(this.currentTrack.Value);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Error("Error while removing queued track with path='{0}'. Exception: {1}", track.Value.Path, ex.Message);
                        }
                    }

                    foreach (var dequeuedTrack in dequeuedTracks)
                    {
                        try
                        {
                            int indexOfCurrentDequeuedTrack = this.playbackOrder.IndexOf(dequeuedTrack.Key);
                            if (indexOfLastDeueuedTrack == 0 | indexOfCurrentDequeuedTrack < indexOfLastDeueuedTrack) indexOfLastDeueuedTrack = indexOfCurrentDequeuedTrack;
                            this.playbackOrder.Remove(dequeuedTrack.Key);
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Error("Error while removing shuffled track with path='{0}'. Exception: {1}", dequeuedTrack.Value.Path, ex.Message);
                        }
                    }
                }
            });

            var dequeueResult = new DequeueResult { IsSuccess = isSuccess, DequeuedTracks = dequeuedTracks, IsPlayingTrackDequeued = isPlayingTrackDequeued };

            if (isSuccess && isPlayingTrackDequeued)
            {
                if (this.playbackOrder.Count == 0)
                {
                    await this.ClearQueueAsync();
                }
                else if (this.playbackOrder.Count > indexOfLastDeueuedTrack)
                {
                    dequeueResult.NextAvailableTrack =
                        new KeyValuePair<string, PlayableTrack>(this.playbackOrder[indexOfLastDeueuedTrack],
                            this.queue[this.playbackOrder[indexOfLastDeueuedTrack]]);
                }
            }

            return dequeueResult;
        }

        public void SetCurrentTrack(KeyValuePair<string, PlayableTrack> track)
        {
            if (!string.IsNullOrEmpty(track.Key))
            {
                this.currentTrack = track;
            }
            else
            {
                // The first matching track
                this.currentTrack = this.queue.Select(t => t).Where(t => t.Value.Equals(track.Value)).FirstOrDefault();
            }
        }

        public async Task<bool> UpdateQueueOrderAsync(List<KeyValuePair<string, PlayableTrack>> tracks, bool isShuffled)
        {
            if (tracks == null || tracks.Count == 0) return false;

            bool isSuccess = true;

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        this.queue.Clear();

                        foreach (KeyValuePair<string, PlayableTrack> track in tracks)
                        {
                            this.queue.Add(track.Key, track.Value);
                        }

                        if (!isShuffled)
                        {
                            this.playbackOrder = new List<string>(this.queue.Keys);
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

        public async Task<UpdateQueueMetadataResult> UpdateMetadataAsync(List<FileMetadata> fileMetadatas)
        {
            var result = new UpdateQueueMetadataResult();

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.Queue != null)
                    {
                        foreach (PlayableTrack track in this.queue.Values)
                        {
                            FileMetadata fmd = fileMetadatas.Select(f => f).Where(f => f.SafePath == track.SafePath).FirstOrDefault();

                            if (fmd != null)
                            {
                                // Queue
                                if (this.UpdateTrackPlaybackInfo(track, fmd))
                                {
                                    result.IsQueueChanged = true;

                                    // Playing track
                                    if (track.SafePath.Equals(this.currentTrack.Value.SafePath))
                                    {
                                        result.IsPlayingTrackPlaybackInfoChanged = true;

                                        // Playing track artwork
                                        if (fmd.ArtworkData.IsValueChanged)
                                        {
                                            result.IsPlayingTrackArtworkChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return result;
        }
        #endregion
    }
}
