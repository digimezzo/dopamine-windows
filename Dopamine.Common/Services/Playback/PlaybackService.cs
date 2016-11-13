using Dopamine.Common.Services.Equalizer;
using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Services.Playback
{
    public class PlaybackService : IPlaybackService
    {
        #region Variables
        private string playingFile;
        private System.Timers.Timer progressTimer = new System.Timers.Timer();
        private double progressTimeoutSeconds = 0.5;
        private double progress = 0.0;
        private float volume = 0.0f;
        private int latency;
        private bool eventMode;
        private bool exclusiveMode;
        private LoopMode loopMode;
        private bool shuffle;
        private bool mute;
        private bool isPlayingPreviousTrack;
        private bool isSpectrumVisible;
        private IPlayer player;

        private IEqualizerService equalizerService;
        private EqualizerPreset desiredPreset;
        private EqualizerPreset activePreset;
        private bool isEqualizerEnabled;

        private IPlayerFactory playerFactory;
        private object queueSyncObject = new object();
        private List<string> queuedPaths = new List<string>(); // The list of queued paths in original order
        private List<string> shuffledPaths = new List<string>(); // The list of queued paths in original order or shuffled

        private ITrackRepository trackRepository;

        private IQueuedTrackRepository queuedTrackRepository;
        private System.Timers.Timer saveQueuedPathssTimer = new System.Timers.Timer();
        private int saveQueuedPathsTimeoutSeconds = 5;

        private bool isSavingQueuedPaths = false;
        private System.Timers.Timer saveTrackStatisticsTimer = new System.Timers.Timer();
        private int saveTrackStatisticsTimeoutSeconds = 5;

        private bool isSavingTrackStatistics = false;
        private Dictionary<string, TrackStatistic> trackStatistics = new Dictionary<string, TrackStatistic>();

        private object trackStatisticsSyncObject = new object();

        private SynchronizationContext context;
        private bool isLoadingTrack;
        #endregion

        #region Properties
        public bool IsSavingQueuedTracks
        {
            get { return this.isSavingQueuedPaths; }
        }

        public bool IsSavingTrackStatistics
        {
            get { return this.isSavingTrackStatistics; }
        }

        public bool NeedsSavingQueuedTracks
        {
            get { return this.saveQueuedPathssTimer.Enabled; }
        }

        public bool NeedsSavingTrackStatistics
        {
            get { return this.trackStatistics.Count > 0; }
        }

        public bool IsStopped
        {
            get
            {
                if (this.player != null)
                {
                    return !this.player.CanStop;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (this.player != null)
                {
                    return this.player.CanPause;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<string> Queue
        {
            get { return this.shuffledPaths; }
        }

        public string PlayingFile
        {
            get
            {
                if (this.playingFile != null)
                {
                    return this.playingFile;
                }
                else if (this.shuffledPaths != null && this.shuffledPaths.Count > 0)
                {
                    return this.shuffledPaths.First();
                }
                else
                {
                    return null;
                }
            }
        }

        public double Progress
        {
            get { return this.progress; }
            set { this.progress = value; }
        }

        public float Volume
        {
            get { return this.volume; }

            set
            {
                if (value > 1)
                {
                    value = 1;
                }

                if (value < 0)
                {
                    value = 0;
                }

                this.volume = value;

                if (this.player != null && !this.mute)
                {
                    this.player.SetVolume(value);
                }

                this.PlaybackVolumeChanged(this, new EventArgs());
            }
        }

        public LoopMode LoopMode
        {
            get { return this.loopMode; }
            set
            {
                this.loopMode = value;
                this.PlaybackLoopChanged(this, new EventArgs());
            }
        }

        public bool Shuffle
        {
            get { return this.shuffle; }
        }

        public bool Mute
        {
            get { return this.mute; }
        }

        public async Task SetShuffle(bool isShuffled)
        {
            this.shuffle = isShuffled;

            if (isShuffled)
            {
                await this.ShuffleTracks();
            }
            else
            {
                await this.UnShuffleTracks();
            }

            this.PlaybackShuffleChanged(this, new EventArgs());
        }

        public int Latency
        {
            get { return this.latency; }
            set { this.latency = value; }
        }

        public bool EventMode
        {
            get { return this.eventMode; }
            set { this.eventMode = value; }
        }

        public bool ExclusiveMode
        {
            get { return this.exclusiveMode; }
            set { this.exclusiveMode = value; }
        }

        public bool IsSpectrumVisible
        {
            get { return this.isSpectrumVisible; }
            set
            {
                this.isSpectrumVisible = value;
                this.SpectrumVisibilityChanged(value);
            }
        }

        public TimeSpan GetCurrentTime
        {
            get
            {
                // Check if there is a file playing
                if (this.player != null && this.player.CanStop)
                {
                    // This prevents displaying a current time which is larger than the total time
                    if (this.player.GetCurrentTime() <= this.player.GetTotalTime())
                    {
                        return this.player.GetCurrentTime();
                    }
                    else
                    {
                        return this.player.GetTotalTime();
                    }
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public TimeSpan GetTotalTime
        {
            get
            {
                // Check if there is a file playing
                if (this.player != null && this.player.CanStop)
                {
                    return this.player.GetTotalTime();
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public IPlayer Player
        {
            get { return this.player; }
        }
        #endregion

        #region Construction
        public PlaybackService(ITrackRepository trackRepository, IQueuedTrackRepository queuedTrackRepository, IEqualizerService equalizerService)
        {
            this.trackRepository = trackRepository;
            this.queuedTrackRepository = queuedTrackRepository;
            this.equalizerService = equalizerService;

            this.context = SynchronizationContext.Current;

            // Initialize the PlayerFactory
            this.playerFactory = new PlayerFactory();

            // Set up timers
            this.progressTimer.Interval = TimeSpan.FromSeconds(this.progressTimeoutSeconds).TotalMilliseconds;
            this.progressTimer.Elapsed += new ElapsedEventHandler(this.ProgressTimeoutHandler);

            this.saveQueuedPathssTimer.Interval = TimeSpan.FromSeconds(this.saveQueuedPathsTimeoutSeconds).TotalMilliseconds;
            this.saveQueuedPathssTimer.Elapsed += new ElapsedEventHandler(this.SaveQueuedTracksTimeoutHandler);

            this.saveTrackStatisticsTimer.Interval = TimeSpan.FromSeconds(this.saveTrackStatisticsTimeoutSeconds).TotalMilliseconds;
            this.saveTrackStatisticsTimer.Elapsed += new ElapsedEventHandler(this.SaveTrackStatisticsHandler);

            // Equalizer
            this.SetIsEqualizerEnabled(XmlSettingsClient.Instance.Get<bool>("Equalizer", "IsEnabled"));

            // Queued tracks
            this.GetSavedQueuedPathsAsync();
        }
        #endregion

        #region Events
        public event PlaybackFailedEventHandler PlaybackFailed = delegate { };
        public event EventHandler PlaybackPaused = delegate { };
        public event EventHandler PlaybackProgressChanged = delegate { };
        public event EventHandler PlaybackResumed = delegate { };
        public event EventHandler PlaybackStopped = delegate { };
        public event Action<bool> PlaybackSuccess = delegate { };
        public event EventHandler PlaybackVolumeChanged = delegate { };
        public event EventHandler PlaybackMuteChanged = delegate { };
        public event EventHandler PlaybackLoopChanged = delegate { };
        public event EventHandler PlaybackShuffleChanged = delegate { };
        public event Action<bool> SpectrumVisibilityChanged = delegate { };
        public event EventHandler ShuffledTracksChanged = delegate { };
        public event Action<int> AddedTracksToQueue = delegate { };
        public event EventHandler TrackStatisticsChanged = delegate { };
        public event Action<bool> LoadingTrack = delegate { };
        #endregion

        #region IPlaybackService
        public async void SetIsEqualizerEnabled(bool isEnabled)
        {
            this.isEqualizerEnabled = isEnabled;

            this.desiredPreset = await this.equalizerService.GetSelectedPresetAsync();

            if (isEnabled)
            {
                this.activePreset = this.desiredPreset;
            }
            else
            {
                this.activePreset = new EqualizerPreset();
            }

            if (this.player != null) this.player.ApplyFilter(this.activePreset.Bands);
        }

        public void ApplyPreset(EqualizerPreset preset)
        {
            this.desiredPreset = preset;

            if (this.isEqualizerEnabled)
            {
                this.activePreset = desiredPreset;
                if (this.player != null) this.player.ApplyFilter(this.activePreset.Bands);
            }
        }

        public async Task SaveQueuedTracksAsync()
        {
            this.saveQueuedPathssTimer.Stop();

            this.isSavingQueuedPaths = true;

            IList<string> paths = null;

            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    paths = this.queuedPaths;
                }
            });

            if (paths != null) await this.queuedTrackRepository.SaveQueuedPathsAsync(paths);

            LogClient.Instance.Logger.Info("Saved queued paths");

            this.isSavingQueuedPaths = false;
        }

        public async Task SaveTrackStatisticsAsync()
        {
            if (this.trackStatistics.Count == 0 | this.isSavingTrackStatistics) return;

            this.saveTrackStatisticsTimer.Stop();

            this.isSavingTrackStatistics = true;

            IList<TrackStatistic> stats = null;

            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    stats = new List<TrackStatistic>(this.trackStatistics.Values);
                    this.trackStatistics.Clear();
                }
            });

            await this.trackRepository.SaveTrackStatisticsAsync(stats);

            this.TrackStatisticsChanged(this, new EventArgs());

            LogClient.Instance.Logger.Info("Saved Track Statistics");

            this.isSavingTrackStatistics = false;

            // If, in the meantime, new statistics are available, reset the timer.
            if (this.trackStatistics.Count > 0)
            {
                this.ResetSaveTrackStatisticsTimer();
            }
        }

        public async Task PlayOrPauseAsync()
        {
            if (!this.IsStopped)
            {
                if (this.IsPlaying)
                {
                    await this.PauseAsync();
                }
                else
                {
                    await this.ResumeAsync();
                }
            }
            else
            {
                if (this.shuffledPaths != null && this.shuffledPaths.Count > 0)
                {
                    // There are already files enqueued. Start playing immediately.
                    await this.PlayFirstAsync();
                }
                else
                {
                    // Enqueue all files before playing
                    await this.Enqueue();
                }
            }
        }

        public void SetMute(bool mute)
        {
            this.mute = mute;
            this.ToggleMute();
        }

        public void Skip(double progress)
        {
            if (this.player != null && this.player.CanStop)
            {
                this.Progress = progress;
                int newSeconds = Convert.ToInt32(progress * this.player.GetTotalTime().TotalSeconds);
                this.player.Skip(newSeconds);
            }
            else
            {
                this.Progress = 0.0;
            }

            this.PlaybackProgressChanged(this, new EventArgs());
        }

        public void Stop()
        {
            if (this.player != null && this.player.CanStop)
            {
                this.player.Stop();
            }

            this.progressTimer.Stop();
            this.Progress = 0.0;
            this.PlaybackStopped(this, new EventArgs());
        }

        public async Task PlayNextAsync()
        {
            try
            {
                if (this.playingFile != null)
                {
                    int currentTime = this.GetCurrentTime.Seconds;
                    int totalTime = this.GetTotalTime.Seconds;

                    if (currentTime <= 10)
                    {
                        this.UpdateTrackStatisticsAsync(this.playingFile, false, true); // Increase SkipCount
                    }
                    else
                    {
                        this.UpdateTrackStatisticsAsync(this.playingFile, true, false); // Increase PlayCount
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not get time information for Track with path='{0}'. Exception: {1}", this.playingFile, ex.Message);
            }

            // We don't want interruptions when trying to play the next Track.
            // If the next Track cannot be played, keep skipping to the 
            // following Track until a working Track is found.
            bool playSuccess = false;
            int numberSkips = 0;

            while (!playSuccess)
            {
                // We skip maximum 3 times. This prevents an infinite 
                // loop if shuffledTracks only contains broken Tracks.
                if (numberSkips < 3)
                {
                    numberSkips += 1;
                    playSuccess = await this.TryPlayNextAsync();
                }
                else
                {
                    this.Stop();
                    playSuccess = true; // Otherwise we never get out of this While loop
                }
            }
        }

        public async Task PlayPreviousAsync()
        {
            // We don't want interruptions when trying to play the previous Track. 
            // If the previous Track cannot be played, keep skipping to the
            // preceding Track until a working Track is found.
            bool playSuccess = false;
            int numberSkips = 0;

            while (!playSuccess)
            {
                // We skip maximum 3 times. This prevents an infinite 
                // loop if shuffledTracks only contains broken Tracks.
                if (numberSkips < 3)
                {
                    numberSkips += 1;
                    playSuccess = await this.TryPlayPreviousAsync();
                }
                else
                {
                    this.Stop();
                    playSuccess = true; // Otherwise we never get out of this While loop
                }
            }
        }

        public async Task Enqueue()
        {
            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(mergedTracks.Select(t => t.Path).ToList());
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(List<string> paths)
        {
            if (paths == null) return;

            await this.EnqueueIfRequired(paths);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(List<string> paths, string selectedPath)
        {
            if (paths == null || selectedPath == null) return;

            await this.EnqueueIfRequired(paths);
            await this.TryPlayAsync(selectedPath);
        }

        public async Task Enqueue(Artist artist)
        {
            if (artist == null) return;

            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(artist.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(mergedTracks.Select(t => t.Path).ToList());
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Genre genre)
        {
            if (genre == null) return;

            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(genre.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(mergedTracks.Select(t => t.Path).ToList());
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Album album)
        {
            if (album == null) return;

            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(album.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(mergedTracks.Select(t => t.Path).ToList());
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Playlist playlist)
        {
            if (playlist == null) return;

            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(playlist.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(mergedTracks.Select(t => t.Path).ToList());
            await this.PlayFirstAsync();
        }

        public async Task PlaySelectedAsync(string selectedPath)
        {
            await this.TryPlayAsync(selectedPath);
        }

        public async Task<DequeueResult> Dequeue(IList<string> selectedPaths)
        {
            bool isSuccess = true;
            var removedQueuedPaths = new List<string>();
            var removedShuffledPaths = new List<string>();
            int smallestIndex = 0;
            bool playNext = false;

            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    foreach (string path in selectedPaths)
                    {
                        try
                        {
                            // Remove from this.queuedPaths. The index doesn't matter.
                            if (this.queuedPaths.Contains(path))
                            {
                                this.queuedPaths.Remove(path);
                                removedQueuedPaths.Add(path);
                            }
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Instance.Logger.Error("Error while removing queued path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }

                    foreach (string path in removedQueuedPaths)
                    {
                        // Remove from this.shuffledPaths. The index does matter,
                        // as we might have to play the next remaining Track.
                        try
                        {
                            int index = this.shuffledPaths.IndexOf(path);

                            if (index >= 0)
                            {
                                if (this.shuffledPaths[index].Equals(this.playingFile))
                                {
                                    playNext = true;
                                }

                                string removedShuffledPath = this.shuffledPaths[index];
                                this.shuffledPaths.RemoveAt(index);
                                removedShuffledPaths.Add(removedShuffledPath);
                                if (smallestIndex == 0 || (index < smallestIndex)) smallestIndex = index;
                            }
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            LogClient.Instance.Logger.Error("Error while removing shuffled path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
            });

            if (playNext & isSuccess)
            {
                if (this.shuffledPaths.Count > smallestIndex)
                {
                    await this.TryPlayAsync(this.shuffledPaths[smallestIndex]);
                }
                else
                {
                    this.Stop();
                }
            }

            var dequeueResult = new DequeueResult { IsSuccess = isSuccess, DequeuedPaths = removedShuffledPaths };

            this.ShuffledTracksChanged(this, new EventArgs());

            this.ResetSaveQueuedPathsTimer(); // Save queued files to the database

            return dequeueResult;
        }

        public async Task<AddToQueueResult> AddToQueue(IList<string> paths)
        {
            var result = new AddToQueueResult { IsSuccess = true };

            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueSyncObject)
                    {
                        result.AddedPaths = paths.Except(this.queuedPaths).ToList();
                        this.queuedPaths.AddRange(result.AddedPaths);
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    LogClient.Instance.Logger.Error("Error while adding paths to queue. Exception: {0}", ex.Message);
                }
            });

            await this.SetPlaybackSettingsAsync();

            if (result.AddedPaths != null)
            {
                if (AddedTracksToQueue != null)
                {
                    AddedTracksToQueue(result.AddedPaths.Count);
                }
            }

            this.ResetSaveQueuedPathsTimer(); // Save queued paths to the database

            return result;
        }

        public async Task<AddToQueueResult> AddToQueue(IList<Artist> artists)
        {
            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(artists), TrackOrder.ByAlbum);
            return await this.AddToQueue(mergedTracks.Select(t => t.Path).ToList());
        }

        public async Task<AddToQueueResult> AddToQueue(IList<Genre> genres)
        {
            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(genres), TrackOrder.ByAlbum);
            return await this.AddToQueue(mergedTracks.Select(t => t.Path).ToList());
        }

        public async Task<AddToQueueResult> AddToQueue(IList<Album> albums)
        {
            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(albums), TrackOrder.ByAlbum);
            return await this.AddToQueue(mergedTracks.Select(t => t.Path).ToList());
        }

        public async Task<AddToQueueResult> AddToQueue(IList<Playlist> playlists)
        {
            List<MergedTrack> mergedTracks = await Utils.OrderMergedTracksAsync(await this.trackRepository.GetMergedTracksAsync(playlists), TrackOrder.ByAlbum);
            return await this.AddToQueue(mergedTracks.Select(t => t.Path).ToList());
        }
        #endregion

        #region Private
        private void SaveTrackStatisticsHandler(object sender, ElapsedEventArgs e)
        {
            this.SaveTrackStatisticsAsync();
        }

        private async Task UpdateTrackStatisticsAsync(string path, bool incrementPlayCount, bool incrementSkipCount)
        {
            await Task.Run(() =>
            {
                lock (this.trackStatisticsSyncObject)
                {
                    try
                    {
                        if (!this.trackStatistics.ContainsKey(path))
                        {
                            this.trackStatistics.Add(path, new TrackStatistic { Path = path });
                        }

                        if (incrementPlayCount)
                        {
                            this.trackStatistics[path].PlayCount += 1;
                            this.trackStatistics[path].DateLastPlayed = DateTime.Now.Ticks;
                        }
                        if (incrementSkipCount) this.trackStatistics[path].SkipCount += 1;
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not update playback statistics for track with path='{0}'. Exception: {1}", path, ex.Message);
                    }
                }
            });

            this.ResetSaveTrackStatisticsTimer();
        }

        private async Task PauseAsync()
        {

            if (this.player != null)
            {
                await Task.Run(() => this.player.Pause());
                this.PlaybackPaused(this, new EventArgs());
            }
        }

        private async Task ResumeAsync()
        {
            if (this.player != null)
            {
                bool isResumed = false;
                await Task.Run(() => isResumed = this.player.Resume());

                if (isResumed)
                {
                    this.PlaybackResumed(this, new EventArgs());
                }
                else
                {
                    this.PlaybackStopped(this, new EventArgs());
                }
            }
        }

        private async Task PlayFirstAsync()
        {
            if (this.shuffledPaths.Count > 0)
            {
                await this.TryPlayAsync(this.shuffledPaths.First());
            }
        }

        private async Task ShuffleTracks()
        {
            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    if (this.queuedPaths.Count > 0)
                    {
                        // To make sure the original queuedFiles doesn't get cleared when randomizing, we first
                        // create a new list by calling ListFunctions.CopyList(queuedFiles) before we randomize.
                        this.shuffledPaths = new List<string>(this.queuedPaths).Randomize();
                    }
                }
            });

            this.ShuffledTracksChanged(this, new EventArgs());
        }

        private async Task UnShuffleTracks()
        {
            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    this.shuffledPaths = new List<string>(this.queuedPaths);
                }
            });

            this.ShuffledTracksChanged(this, new EventArgs());
        }

        private void ToggleMute()
        {
            if (this.player != null)
            {
                if (this.mute)
                {
                    this.player.SetVolume(0.0f);
                }
                else
                {
                    this.player.SetVolume(this.Volume);
                }
            }

            this.PlaybackMuteChanged(this, new EventArgs());
        }

        private async Task<bool> TryPlayAsync(string filename)
        {
            bool isPlaybackSuccess = true;
            PlaybackFailedEventArgs playbackFailedEventArgs = null;

            if (this.isLoadingTrack) return isPlaybackSuccess;

            this.isLoadingTrack = true;
            this.LoadingTrack(this.isLoadingTrack);

            try
            {
                // If a Track was playing, make sure it is now stopped
                if (this.player != null)
                {
                    // Remove the previous Stopped handler (not sure this is needed)
                    this.player.PlaybackInterrupted -= this.PlaybackInterruptedHandler;
                    this.player.PlaybackFinished -= this.PlaybackFinishedHandler;

                    this.player.Stop();
                    this.player.Dispose();
                    this.player = null;
                }

                // Check that the file exists
                if (!System.IO.File.Exists(filename))
                {
                    throw new FileNotFoundException(string.Format("File '{0}' was not found", filename));
                }

                // Play the Track from its runtime path (current or temporary)
                this.player = this.playerFactory.Create(Path.GetExtension(filename));

                this.player.SetOutputDevice(this.Latency, this.EventMode, this.ExclusiveMode, this.activePreset.Bands);

                this.ToggleMute();

                // We need to set PlayingFile before trying to play the file.
                // So if we go into the Catch when trying to play the file,
                // at least, the next time TryPlayNext is called, it will know that 
                // we already tried to play this file and it can find the next file.
                this.playingFile = filename;

                // Play the Track
                await Task.Run(() => this.player.Play(filename));

                // Start reporting progress
                this.progressTimer.Start();

                // Hook up the Stopped event
                this.player.PlaybackInterrupted += this.PlaybackInterruptedHandler;
                this.player.PlaybackFinished += this.PlaybackFinishedHandler;

                // Playing was successful
                this.PlaybackSuccess(this.isPlayingPreviousTrack);

                // Set this to false again after raising the event. It is important to have a correct slide 
                // direction for cover art when the next Track is a file from double click in Windows.
                this.isPlayingPreviousTrack = false;
            }
            catch (FileNotFoundException fnfex)
            {
                playbackFailedEventArgs = new PlaybackFailedEventArgs { FailureReason = PlaybackFailureReason.FileNotFound, Message = fnfex.Message, StackTrace = fnfex.StackTrace };
                isPlaybackSuccess = false;
            }
            catch (Exception ex)
            {
                playbackFailedEventArgs = new PlaybackFailedEventArgs { FailureReason = PlaybackFailureReason.Unknown, Message = ex.Message, StackTrace = ex.StackTrace };
                isPlaybackSuccess = false;
            }

            if (!isPlaybackSuccess)
            {
                try
                {
                    if (this.player != null) this.player.Stop();
                }
                catch (Exception)
                {
                    LogClient.Instance.Logger.Error("Could not stop the Player");
                }

                LogClient.Instance.Logger.Error("Could not play the file {0}. EventMode={1}, ExclusiveMode={2}, LoopMode={3}, Shuffle={4}. Exception: {5}. StackTrace: {6}", filename, this.eventMode, this.exclusiveMode, this.LoopMode.ToString(), this.shuffle, playbackFailedEventArgs.Message, playbackFailedEventArgs.StackTrace);

                this.PlaybackFailed(this, playbackFailedEventArgs);
            }

            this.isLoadingTrack = false;
            this.LoadingTrack(this.isLoadingTrack);

            return isPlaybackSuccess;
        }

        private async Task<bool> TryPlayNextAsync()
        {
            this.isPlayingPreviousTrack = false;

            string trackToPlay = null;

            lock (this.queueSyncObject)
            {
                if (this.shuffledPaths != null && this.shuffledPaths.Count > 0)
                {
                    int firstIndex = 0;
                    int lastIndex = this.shuffledPaths.Count - 1;
                    int playingTrackIndex = this.shuffledPaths.IndexOf(this.playingFile);

                    if (this.LoopMode == LoopMode.One)
                    {
                        // Play the same file again
                        trackToPlay = this.shuffledPaths[playingTrackIndex];
                    }
                    else
                    {
                        if (playingTrackIndex < lastIndex)
                        {
                            // If we didn't reach the end of the list, try to play the next file.
                            trackToPlay = this.shuffledPaths[playingTrackIndex + 1];
                        }
                        else if (this.LoopMode == LoopMode.All)
                        {
                            // When LoopMode.All is enabled, when we reach the end of the list, start from the beginning.
                            trackToPlay = this.shuffledPaths[firstIndex];
                        }
                        else
                        {
                            // In any other case, stop playing.
                            this.Stop();
                        }
                    }
                }
            }

            return trackToPlay != null ? await this.TryPlayAsync(trackToPlay) : true;
        }

        private async Task<bool> TryPlayPreviousAsync()
        {
            this.isPlayingPreviousTrack = true;

            string trackToPlay = null;

            lock (this.queueSyncObject)
            {
                if (this.shuffledPaths != null && this.shuffledPaths.Count > 0)
                {
                    int firstIndex = 0;
                    int lastIndex = this.shuffledPaths.Count - 1;
                    int playingTrackIndex = this.shuffledPaths.IndexOf(this.playingFile);

                    if (this.GetCurrentTime.Seconds > 3)
                    {
                        // If we're more than 3 seconds into the file, try to
                        // jump to the beginning of the current file.
                        this.player.Skip(0);
                        return true;
                    }
                    else
                    {
                        // If we're less than 3 seconds into the file, we have to check some things...
                        if (this.LoopMode == LoopMode.One)
                        {
                            // Play the same Track again
                            trackToPlay = this.shuffledPaths[playingTrackIndex];
                        }
                        else
                        {
                            if (playingTrackIndex > firstIndex)
                            {
                                // If we didn't reach the start of the list, try to play the previous file.
                                trackToPlay = this.shuffledPaths[playingTrackIndex - 1];
                            }
                            else if (this.LoopMode == LoopMode.All)
                            {
                                // When LoopMode.All is enabled, when we reach the start of the list, start from the end.
                                trackToPlay = this.shuffledPaths[lastIndex];
                            }
                            else
                            {
                                // In any other case, stop playing.
                                this.Stop();
                            }
                        }
                    }
                }
            }

            return trackToPlay != null ? await this.TryPlayAsync(trackToPlay) : true;
        }

        private void ProgressTimeoutHandler(object sender, ElapsedEventArgs e)
        {
            this.HandleProgress();
        }

        private void PlaybackInterruptedHandler(Object sender, PlaybackInterruptedEventArgs e)
        {
            LogClient.Instance.Logger.Error("Playback of track '{0}' was interrupted. Trying to play the next track anyway. Exception: {1}", this.playingFile, e.Message);

            // Try to play the next file from the list automatically.
            // Use our context to trigger the work, because this event is fired on the Player's Playback thread.
            this.context.Post(new SendOrPostCallback(async (state) => await this.TryPlayNextAsync()), null);
        }

        private void PlaybackFinishedHandler(Object sender, EventArgs e)
        {
            // Try to play the next file from the list automatically
            // Use our context to trigger the work, because this event is fired on the Player's Playback thread.
            this.context.Post(new SendOrPostCallback(async (state) =>
            {
                await this.UpdateTrackStatisticsAsync(this.playingFile, true, false); // Increase PlayCount
                await this.TryPlayNextAsync();
            }), null);
        }

        private void SaveQueuedTracksTimeoutHandler(object sender, ElapsedEventArgs e)
        {
            this.SaveQueuedTracksAsync();
        }

        private async Task GetSavedQueuedPathsAsync()
        {
            List<string> savedQueuedFiles = await this.queuedTrackRepository.GetSavedQueuedPathsAsync();

            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    // It could be that, while getting saved queued files from the database above, 
                    // files were enqueued from the command line. To prevent overwriting the existing 
                    // queue (which was built based on command line files), we check if the queue is
                    // empty first, and fill it up with saved queued files only if it is empty.
                    if (this.queuedPaths == null || this.queuedPaths.Count == 0) this.queuedPaths = savedQueuedFiles;
                }
            });

            await this.SetPlaybackSettingsAsync();
        }

        private void HandleProgress()
        {
            if (this.player != null && this.player.CanStop)
            {
                TimeSpan totalTime = this.player.GetTotalTime();
                TimeSpan currentTime = this.player.GetCurrentTime();

                this.Progress = currentTime.TotalMilliseconds / totalTime.TotalMilliseconds;
            }
            else
            {
                this.Progress = 0.0;
            }

            if (PlaybackProgressChanged != null)
            {
                PlaybackProgressChanged(this, null);
            }
        }

        private async Task EnqueueIfRequired(List<string> paths)
        {
            bool needsEnqueue = false;

            await Task.Run(() =>
            {
                lock (this.queueSyncObject)
                {
                    if (this.queuedPaths == null || this.queuedPaths.Count == 0 || paths.Count != this.queuedPaths.Count)
                    {
                        needsEnqueue = true;
                    }
                    else if (this.queuedPaths.Except(paths).ToList().Count != 0)
                    {
                        needsEnqueue = true;
                    }
                }
            });


            if (needsEnqueue)
            {
                lock (this.queueSyncObject)
                {
                    this.queuedPaths = paths;
                }

                await this.SetPlaybackSettingsAsync();
            }

            this.ResetSaveQueuedPathsTimer(); // Save queued tracks to the database
        }

        private void ResetSaveQueuedPathsTimer()
        {
            this.saveQueuedPathssTimer.Stop();
            this.saveQueuedPathssTimer.Start();
        }


        private void ResetSaveTrackStatisticsTimer()
        {
            this.saveTrackStatisticsTimer.Stop();
            this.saveTrackStatisticsTimer.Start();
        }

        private async Task SetPlaybackSettingsAsync()
        {
            this.LoopMode = (LoopMode)XmlSettingsClient.Instance.Get<int>("Playback", "LoopMode");
            this.Latency = XmlSettingsClient.Instance.Get<int>("Playback", "AudioLatency");
            this.Volume = XmlSettingsClient.Instance.Get<float>("Playback", "Volume");
            this.mute = XmlSettingsClient.Instance.Get<bool>("Playback", "Mute");
            this.EventMode = false;
            //this.EventMode = XmlSettingsClient.Instance.Get<bool>("Playback", "WasapiEventMode");
            //this.ExclusiveMode = false;
            this.ExclusiveMode = XmlSettingsClient.Instance.Get<bool>("Playback", "WasapiExclusiveMode");

            await SetShuffle(XmlSettingsClient.Instance.Get<bool>("Playback", "Shuffle"));
        }
        #endregion
    }
}
