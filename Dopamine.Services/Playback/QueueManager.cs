using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Playback
{
    internal class QueueManager
    {
        private ITrackRepository trackRepository;
        private TrackViewModel currentTrack;
        private object queueLock = new object();
        private List<TrackViewModel> queue = new List<TrackViewModel>(); // Queued tracks in original order
        private List<int> playbackOrder = new List<int>(); // Playback order of queued tracks (Contains the indexes of list the queued tracks)

        public QueueManager(ITrackRepository trackRepository)
        {
            this.trackRepository = trackRepository;
        }

        public IList<TrackViewModel> Queue
        {
            get { return this.queue; }
        }

        private List<int> GetQueueIndices()
        {
            if (this.queue != null)
            {
                return Enumerable.Range(0, this.queue.Count).ToList();
            }

            return new List<int>();
        }

        private int FindQueueIndex(TrackViewModel track)
        {
            if (this.queue != null)
            {
                return this.queue.IndexOf(track);
            }

            return 0;
        }

        private int FindPlaybackOrderIndex(TrackViewModel track)
        {
            if (this.queue != null && this.playbackOrder != null)
            {
                int queueIndex = this.queue.IndexOf(track);
                return this.playbackOrder.IndexOf(queueIndex);
            }

            return 0;
        }

        public async Task ShuffleAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.queue.Count > 0)
                    {
                        if (this.currentTrack != null || !this.queue.Contains(this.currentTrack))
                        {
                            // We're not playing a track from the queue: just shuffle.
                            this.playbackOrder = this.GetQueueIndices().Randomize();
                        }
                        else
                        {
                            // We're playing a track from the queue: shuffle, but make sure the playing track comes first.
                            int currentTrackIndex = this.FindQueueIndex(this.currentTrack);
                            this.playbackOrder = new List<int>();
                            this.playbackOrder.Add(currentTrackIndex);
                            List<int> tempPlaybackOrder = this.GetQueueIndices();
                            tempPlaybackOrder.Remove(currentTrackIndex);
                            this.playbackOrder.AddRange(tempPlaybackOrder.Randomize());
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
                        this.playbackOrder = this.GetQueueIndices();
                    }
                }
            });
        }

        public TrackViewModel CurrentTrack()
        {
            try
            {
                if (this.currentTrack != null)
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

            return null;
        }

        public TrackViewModel FirstTrack()
        {
            TrackViewModel firstTrack = null;

            try
            {
                if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                {
                    firstTrack = this.queue[this.playbackOrder.First()];
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get first track. Exception: {0}", ex.Message);
            }

            return firstTrack;
        }

        public async Task<TrackViewModel> PreviousTrackAsync(LoopMode loopMode)
        {
            TrackViewModel previousTrack = null;

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.FindPlaybackOrderIndex(this.currentTrack);

                            if (loopMode == LoopMode.One)
                            {
                                // Return the current track
                                previousTrack = this.currentTrack;
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

        public async Task<TrackViewModel> NextTrackAsync(LoopMode loopMode, bool returnToStart)
        {
            TrackViewModel nextTrack = null;

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLock)
                    {
                        if (this.playbackOrder != null && this.playbackOrder.Count > 0)
                        {
                            int currentTrackIndex = this.FindPlaybackOrderIndex(this.currentTrack);

                            if (loopMode.Equals(LoopMode.One))
                            {
                                // Return the current track
                                nextTrack = this.queue[this.playbackOrder[currentTrackIndex]];
                            }
                            else
                            {
                                if (currentTrackIndex < this.playbackOrder.Count - 1)
                                {
                                    // If we didn't reach the end of the queue, return the next track.
                                    int increment = 1;

                                    nextTrack = this.queue[this.playbackOrder[currentTrackIndex + increment]];

                                    // HACK: voids getting stuck on the same track when the playlist contains the same track multiple times
                                    while (this.currentTrack.Path.Equals(nextTrack.Path))
                                    {
                                        increment++;
                                        nextTrack = this.queue[this.playbackOrder[currentTrackIndex + increment]];
                                    }
                                }
                                else if (loopMode.Equals(LoopMode.All) | returnToStart)
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
                            this.queue.Add(track.DeepCopy());
                        }

                        result.EnqueuedTracks = tracks;
                    }
                });

                if (shuffle)
                {
                    await this.ShuffleAsync();
                }
                else
                {
                    await this.UnShuffleAsync();
                }
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
                        int playbackOrderCount = this.playbackOrder.Count;

                        if (this.currentTrack != null)
                        {
                            queueIndex = this.FindQueueIndex(this.currentTrack);
                            playbackOrderIndex = this.FindPlaybackOrderIndex(this.currentTrack);
                        }

                        var tracksToAdd = new List<TrackViewModel>();

                        foreach (TrackViewModel track in tracks)
                        {
                            tracksToAdd.Add(track.DeepCopy());
                        }

                        this.queue.InsertRange(queueIndex + 1, tracksToAdd);

                        for (int i = 0; i < this.playbackOrder.Count; i++)
                        {
                            if (this.playbackOrder[i] > queueIndex)
                            {
                                this.playbackOrder[i] += tracksToAdd.Count;
                            }
                        }

                        this.playbackOrder.InsertRange(playbackOrderIndex + 1, Enumerable.Range(queueIndex + 1, tracksToAdd.Count));

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
                        this.currentTrack = null;
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

        public async Task<DequeueResult> DequeueAsync(IList<TrackViewModel> tracks)
        {
            bool isSuccess = true;
            bool isPlayingTrackDequeued = false;
            IList<TrackViewModel> dequeuedTracks = new List<TrackViewModel>();
            TrackViewModel nextAvailableTrack = null;

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    try
                    {
                        // First, get the tracks to dequeue and which are in the queue (normally it's all of them. But we're just making sure.)
                        IList<TrackViewModel> tracksToDequeue = this.queue.Where(x => tracks.Contains(x)).ToList();

                        // Then, remove from playbackOrder
                        foreach (TrackViewModel trackToDequeue in tracksToDequeue)
                        {
                            try
                            {
                                nextAvailableTrack = null;

                                try
                                {
                                    nextAvailableTrack = this.queue[this.playbackOrder[this.FindPlaybackOrderIndex(trackToDequeue) + 1]];
                                }
                                catch (Exception)
                                {
                                    // Intended suppression
                                }

                                this.playbackOrder.Remove(this.FindPlaybackOrderIndex(trackToDequeue));
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error($"Error while removing track with path='{trackToDequeue.Path}' from the playbackOrder. Exception: {ex.Message}");
                                throw;
                            }

                        }

                        // Finally, remove from queue
                        foreach (TrackViewModel trackToDequeue in tracksToDequeue)
                        {
                            try
                            {
                                this.queue.Remove(trackToDequeue);
                                isPlayingTrackDequeued = isPlayingTrackDequeued || trackToDequeue.Equals(this.currentTrack);
                                dequeuedTracks.Add(trackToDequeue);
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error($"Error while removing track with path='{trackToDequeue.Path}' from the queue. Exception: {ex.Message}");
                                throw;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error($"Error while removing tracks from the queue. Queue will be cleared. Exception: {ex.Message}");
                        isSuccess = false;
                    }
                }
            });

            if (!isSuccess)
            {
                LogClient.Warning($"Removing tracks from queue failed. Clearing queue.");
                await this.ClearQueueAsync();
                dequeuedTracks = new List<TrackViewModel>(tracks);
            }

            var dequeueResult = new DequeueResult
            {
                IsSuccess = isSuccess,
                DequeuedTracks = dequeuedTracks,
                NextAvailableTrack = nextAvailableTrack,
                IsPlayingTrackDequeued = isPlayingTrackDequeued
            };

            return dequeueResult;
        }

        public void SetCurrentTrack(string path)
        {
            this.currentTrack = this.queue.Where(x=> x.SafePath.Equals(path.ToSafePath())).FirstOrDefault();
        }

        public async Task<bool> UpdateQueueOrderAsync(IList<TrackViewModel> tracks, bool isShuffled)
        {
            if (tracks == null || tracks.Count == 0)
            {
                return false;
            }

            bool isSuccess = true;

            try
            {
                await Task.Run(() =>
                {
                    lock (this.queueLock)
                    {
                        this.queue.Clear();
                        this.queue.AddRange(tracks);

                        if (!isShuffled)
                        {
                            this.playbackOrder = this.GetQueueIndices();
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

        public async Task<UpdateQueueMetadataResult> UpdateMetadataAsync(IList<FileMetadata> fileMetadatas)
        {
            var result = new UpdateQueueMetadataResult();

            IList<Track> tracks = await this.trackRepository.GetTracksAsync(fileMetadatas.Select(x => x.Path).ToList());

            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.Queue != null)
                    {
                        // Queue
                        result.IsQueueChanged = true;

                        foreach (TrackViewModel trackViewModel in this.queue)
                        {
                            Track newTrack = tracks.Where(x => x.SafePath.Equals(trackViewModel.SafePath)).FirstOrDefault();

                            trackViewModel.UpdateTrack(newTrack);

                            // Playing track
                            if (trackViewModel.SafePath.Equals(this.currentTrack.SafePath))
                            {
                                result.IsPlayingTrackChanged = true;
                                this.currentTrack.UpdateTrack(newTrack);
                            }
                        }
                    }
                }
            });

            return result;
        }

        public async Task UpdateQueueLanguageAsync()
        {
            await Task.Run(() =>
            {
                lock (this.queueLock)
                {
                    if (this.Queue != null)
                    {
                        foreach (TrackViewModel trackViewModel in this.queue)
                        {
                            trackViewModel.Refresh();
                        } 
                    }

                    if (this.currentTrack != null)
                    {
                        this.currentTrack.Refresh();
                    }
                }
            });
        }
    }
}
