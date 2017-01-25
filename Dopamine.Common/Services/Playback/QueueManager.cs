using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Extensions;
using Dopamine.Common.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dopamine.Common.Helpers;

namespace Dopamine.Common.Services.Playback
{
    public class QueueManager
    {
        #region Variables
        private string currentTrackGuid;
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
                    if (this.queue.Count > 0)
                    {
                        this.playbackOrder = new List<string>(this.queue.Keys).Randomize();
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

        public PlayableTrack GetTrack(string trackGuid)
        {
            try
            {
                return this.queue[trackGuid];
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get track. Exception: {0}", ex.Message);
            }

            return null;
        }

        public PlayableTrack CurrentTrack()
        {
            try
            {
                if (this.currentTrackGuid != null)
                {
                    return this.queue[this.currentTrackGuid];
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

            return null;
        }

        public string CurrentTrackGuid()
        {
            try
            {
                return this.currentTrackGuid;
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get current trackGuid. Exception: {0}", ex.Message);
            }

            return null;
        }

        public PlayableTrack FirstTrack()
        {
            try
            {
                if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                {
                    return this.queue[this.playbackOrder.First()];
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get first track. Exception: {0}", ex.Message);
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
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.playbackOrder.IndexOf(this.currentTrackGuid);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                previousTrack = this.queue[this.playbackOrder[currentTrackIndex]];
                            }
                            else
                            {
                                if (currentTrackIndex > 0)
                                {
                                    // If we didn't reach the start of the queue, return the previous track.
                                    previousTrack = this.queue[this.playbackOrder[currentTrackIndex - 1]];
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the start of the queue, return the last track.
                                    previousTrack = this.queue[this.playbackOrder.Last()];
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
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.playbackOrder.IndexOf(this.currentTrackGuid);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                nextTrack = this.queue[this.playbackOrder[currentTrackIndex]];
                            }
                            else
                            {
                                if (currentTrackIndex < this.playbackOrder.Count - 1)
                                {
                                    // If we didn't reach the end of the queue, return the next track.
                                    nextTrack = this.queue[this.playbackOrder[currentTrackIndex + 1]];
                                }
                                else if (loopMode == LoopMode.All)
                                {
                                    // When LoopMode.All is enabled, when we reach the end of the queue, return the first track.
                                    nextTrack = this.queue[this.playbackOrder.First()];
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
                        // Only enqueue tracks which are not queued yet
                        List<PlayableTrack> tracksToEnqueue = tracks.Except(this.queue.Values).ToList();

                        foreach (PlayableTrack track in tracksToEnqueue)
                        {
                            this.queue.Add(Guid.NewGuid().ToString(), track);
                        }

                        result.EnqueuedTracks = tracksToEnqueue;
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

        public async Task<EnqueueResult> EnqueueNextAsync(IList<PlayableTrack> tracks, bool shuffle)
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

                        if (this.currentTrackGuid != null)
                        {
                            queueIndex = this.queue.IndexOf(this.currentTrackGuid);
                            playbackOrderIndex = this.playbackOrder.IndexOf(this.currentTrackGuid);
                        }

                        var trackPairs = new List<KeyValuePair<string, PlayableTrack>>();

                        foreach (PlayableTrack track in tracks)
                        {
                            trackPairs.Add(new KeyValuePair<string, PlayableTrack>(Guid.NewGuid().ToString(), track));
                        }

                        this.queue.InsertRange(queueIndex + 1, trackPairs);
                        this.playbackOrder.InsertRange(queueIndex + 1, trackPairs.Select(t => t.Key));

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

        public async Task<DequeueResult> DequeueAsync(IList<PlayableTrack> tracks)
        {
            // TODO
            bool isSuccess = true;
            var removedQueuedTracks = new List<PlayableTrack>();
            var removedShuffledTracks = new List<PlayableTrack>();
            int indexOfLastRemovedQueuedTrack = 0;
            bool isPlayingTrackDequeued = false;

            //await Task.Run(() =>
            //{
            //    lock (this.queueLock)
            //    {
            //        foreach (PlayableTrack t in tracks)
            //        {
            //            try
            //            {
            //                // Remove from this.queuedTracks. The index doesn't matter.
            //                if (this.queuedTracks.Contains(t))
            //                {
            //                    this.queuedTracks.Remove(t);
            //                    removedQueuedTracks.Add(t);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                isSuccess = false;
            //                LogClient.Error("Error while removing queued track with path='{0}'. Exception: {1}", t.Path, ex.Message);
            //            }
            //        }

            //        foreach (PlayableTrack removedQueuedTrack in removedQueuedTracks)
            //        {
            //            // Remove from this.shuffledTracks. The index does matter,
            //            // as we might have to play the next available Track.
            //            try
            //            {
            //                int indexOfCurrentRemovedQueuedTrack = this.shuffledTracks.IndexOf(removedQueuedTrack);

            //                if (indexOfCurrentRemovedQueuedTrack >= 0)
            //                {
            //                    if (this.shuffledTracks[indexOfCurrentRemovedQueuedTrack].Equals(this.currentTrack))
            //                    {
            //                        isPlayingTrackDequeued = true;
            //                    }

            //                    PlayableTrack removedShuffledTrack = this.shuffledTracks[indexOfCurrentRemovedQueuedTrack];
            //                    this.shuffledTracks.RemoveAt(indexOfCurrentRemovedQueuedTrack);
            //                    removedShuffledTracks.Add(removedShuffledTrack);
            //                    if (indexOfCurrentRemovedQueuedTrack < indexOfLastRemovedQueuedTrack) indexOfLastRemovedQueuedTrack = indexOfCurrentRemovedQueuedTrack;
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                isSuccess = false;
            //                LogClient.Error("Error while removing shuffled track with path='{0}'. Exception: {1}", removedQueuedTrack.Path, ex.Message);
            //            }
            //        }
            //    }
            //});

            var dequeueResult = new DequeueResult { IsSuccess = isSuccess, DequeuedTracks = removedShuffledTracks, IsPlayingTrackDequeued = isPlayingTrackDequeued };

            //if (isSuccess & isPlayingTrackDequeued & this.shuffledTracks.Count > indexOfLastRemovedQueuedTrack)
            //{
            //    dequeueResult.NextAvailableTrack = this.shuffledTracks[indexOfLastRemovedQueuedTrack];
            //}

            return dequeueResult;
        }

        public void SetCurrentTrack(PlayableTrack track, string trackGuid)
        {
            if (!string.IsNullOrEmpty(trackGuid))
            {
                this.currentTrackGuid = trackGuid;
            }
            else
            {
                // The first matching track
                this.currentTrackGuid = this.queue.Select(t => t).Where(t => t.Value.Equals(track)).Select(t => t.Key).FirstOrDefault();
            }
        }

        public async Task<bool> UpdateQueueOrderAsync(List<PlayableTrack> tracks, bool isShuffled)
        {
            // TODO
            //if (tracks == null || tracks.Count == 0) return false;

            bool isSuccess = true;

            //try
            //{
            //    await Task.Run(() =>
            //    {
            //        lock (this.queueLock)
            //        {
            //            this.queuedTracks = new List<PlayableTrack>(tracks);

            //            if (!isShuffled)
            //            {
            //                this.shuffledTracks = new List<PlayableTrack>(this.queuedTracks);
            //            }
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    isSuccess = false;
            //    LogClient.Error("Could update queue order. Exception: {0}", ex.Message);
            //}

            return isSuccess;
        }

        public async Task<UpdateQueueMetadataResult> UpdateQueueMetadataAsync(List<FileMetadata> fileMetadatas)
        {
            var result = new UpdateQueueMetadataResult();

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.Queue != null)
                    {
                        bool isQueueChanged = false;

                        foreach (PlayableTrack track in this.queue.Values)
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
