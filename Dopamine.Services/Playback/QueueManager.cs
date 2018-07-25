using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Data.Metadata;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Playback
{
    internal class QueueManager
    {
        private KeyValuePair<string, TrackViewModel> currentTrack;
        private object queueLock = new object();
        private OrderedDictionary<string, TrackViewModel> queue = new OrderedDictionary<string, TrackViewModel>(); // Queued tracks in original order
        private List<string> playbackOrder = new List<string>(); // Playback order of queued tracks
      
        public OrderedDictionary<string, TrackViewModel> Queue
        {
            get { return this.queue; }
        }
    
        private bool UpdateTrackPlaybackInfo(TrackViewModel track, IFileMetadata fileMetadata)
        {
            bool isDisplayedPlaybackInfoChanged = false;

            try
            {
                // Only update the properties that are displayed on Now Playing screens
                if (fileMetadata.Title.IsValueChanged)
                {
                    // TODO track.TrackTitle = fileMetadata.Title.Value;
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Artists.IsValueChanged)
                {
                    // TODO track.ArtistName = fileMetadata.Artists.Values.FirstOrDefault();
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Year.IsValueChanged)
                {
                    // TODO track.Year = fileMetadata.Year.Value.SafeConvertToLong();
                    isDisplayedPlaybackInfoChanged = true;
                }

                if (fileMetadata.Album.IsValueChanged)
                {
                    // TODO track.AlbumTitle = fileMetadata.Album.Value;
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
    
        public async Task ShuffleAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queue.Count > 0)
                    {
                        if (this.currentTrack.Equals(default(KeyValuePair<string, TrackViewModel>)) || !this.queue.Keys.Contains(this.currentTrack.Key))
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

        public KeyValuePair<string, TrackViewModel> CurrentTrack()
        {
            try
            {
                if (!this.currentTrack.Equals(default(KeyValuePair<string, TrackViewModel>)))
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

            return default(KeyValuePair<string, TrackViewModel>);
        }

        public KeyValuePair<string, TrackViewModel> FirstTrack()
        {
            KeyValuePair<string, TrackViewModel> firstTrack = default(KeyValuePair<string, TrackViewModel>);

            try
            {
                if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                {
                    firstTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder.First(), this.queue[this.playbackOrder.First()]);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get first track. Exception: {0}", ex.Message);
            }

            return firstTrack;
        }

        public async Task<KeyValuePair<string, TrackViewModel>> PreviousTrackAsync(LoopMode loopMode)
        {
            KeyValuePair<string, TrackViewModel> previousTrack = default(KeyValuePair<string, TrackViewModel>);

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
                                previousTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder[currentTrackIndex], this.queue[this.playbackOrder[currentTrackIndex]]);
                            }
                            else
                            {
                                if (currentTrackIndex > 0)
                                {
                                    // If we didn't reach the start of the queue, return the previous track.
                                    previousTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder[currentTrackIndex - 1], this.queue[this.playbackOrder[currentTrackIndex - 1]]);
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the start of the queue, return the last track.
                                    previousTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder.Last(), this.queue[this.playbackOrder.Last()]);
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

        public async Task<KeyValuePair<string, TrackViewModel>> NextTrackAsync(LoopMode loopMode, bool returnToStart)
        {
            KeyValuePair<string, TrackViewModel> nextTrack = default(KeyValuePair<string, TrackViewModel>);

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.playbackOrder.IndexOf(this.currentTrack.Key);

                            if (loopMode.Equals(LoopMode.One))
                            {
                                // Return the current track
                                nextTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder[currentTrackIndex], this.queue[this.playbackOrder[currentTrackIndex]]);
                            }
                            else
                            {
                                if (currentTrackIndex < this.playbackOrder.Count - 1)
                                {
                                    // If we didn't reach the end of the queue, return the next track.
                                    nextTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder[currentTrackIndex + 1], this.queue[this.playbackOrder[currentTrackIndex + 1]]);
                                }
                                else if (loopMode.Equals( LoopMode.All) | returnToStart)
                                {
                                    // When LoopMode.All is enabled, when we reach the end of the queue, return the first track.
                                    nextTrack = new KeyValuePair<string, TrackViewModel>(this.playbackOrder.First(), this.queue[this.playbackOrder.First()]);
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

        public async Task<EnqueueResult> EnqueueAsync(IList<TrackViewModel> tracks, bool shuffle)
        {
            var result = new EnqueueResult { IsSuccess = true };

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        foreach (TrackViewModel track in tracks)
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

        public async Task<EnqueueResult> EnqueueAsync(IList<KeyValuePair<string, TrackViewModel>> tracks, bool shuffle)
        {
            var result = new EnqueueResult { IsSuccess = true };

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        foreach (KeyValuePair<string, TrackViewModel> track in tracks)
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

        public async Task<EnqueueResult> EnqueueNextAsync(IList<TrackViewModel> tracks)
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

                        if (!this.currentTrack.Equals(default(KeyValuePair<string, TrackViewModel>)))
                        {
                            queueIndex = this.queue.IndexOf(this.currentTrack.Key);
                            playbackOrderIndex = this.playbackOrder.IndexOf(this.currentTrack.Key);
                        }

                        var kvp = new List<KeyValuePair<string, TrackViewModel>>();

                        foreach (TrackViewModel track in tracks)
                        {
                            kvp.Add(new KeyValuePair<string, TrackViewModel>(Guid.NewGuid().ToString(), track));
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
                        this.currentTrack = default(KeyValuePair<string, TrackViewModel>);
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

        public async Task<bool> IsQueueDifferentAsync(IList<TrackViewModel> tracks)
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

        public async Task<bool> IsQueueDifferentAsync(IList<KeyValuePair<string, TrackViewModel>> tracks)
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
                        foreach (KeyValuePair<string, TrackViewModel> track in tracks)
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

        public async Task<DequeueResult> DequeueAsync(IList<KeyValuePair<string, TrackViewModel>> tracks)
        {
            bool isSuccess = true;
            var dequeuedTracks = new List<KeyValuePair<string, TrackViewModel>>();
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
                        new KeyValuePair<string, TrackViewModel>(this.playbackOrder[indexOfLastDeueuedTrack],
                            this.queue[this.playbackOrder[indexOfLastDeueuedTrack]]);
                }
            }

            return dequeueResult;
        }

        public void SetCurrentTrack(KeyValuePair<string, TrackViewModel> track)
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

        public async Task<bool> UpdateQueueOrderAsync(List<KeyValuePair<string, TrackViewModel>> tracks, bool isShuffled)
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

                        foreach (KeyValuePair<string, TrackViewModel> track in tracks)
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

        public async Task<UpdateQueueMetadataResult> UpdateMetadataAsync(List<IFileMetadata> fileMetadatas)
        {
            var result = new UpdateQueueMetadataResult();

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.Queue != null)
                    {
                        foreach (TrackViewModel track in this.queue.Values)
                        {
                            IFileMetadata fmd = fileMetadatas.Select(f => f).Where(f => f.SafePath == track.SafePath).FirstOrDefault();

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
    }
}
