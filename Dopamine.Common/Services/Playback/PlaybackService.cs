using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Audio;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Equalizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Services.Playback
{
    public class PlaybackService : IPlaybackService
    {
        #region Variables
        private QueueManager queueManager;
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

        private ITrackRepository trackRepository;
        private ITrackStatisticRepository trackStatisticRepository;

        private System.Timers.Timer savePlaybackCountersTimer = new System.Timers.Timer();
        private int savePlaybackCountersTimeoutSeconds = 5;

        private bool isSavingPlaybackCounters = false;
        private Dictionary<string, PlaybackCounter> playbackCounters = new Dictionary<string, PlaybackCounter>();

        private object playbackCountersLock = new object();

        private SynchronizationContext context;
        private bool isLoadingTrack;
        #endregion

        #region Properties
        public bool IsSavingPlaybackCounters
        {
            get { return this.isSavingPlaybackCounters; }
        }

        public bool NeedsSavingPlaybackCounters
        {
            get { return this.playbackCounters.Count > 0; }
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

        public OrderedDictionary<string, PlayableTrack> Queue
        {
            get { return this.queueManager.Queue; }
        }

        public PlayableTrack PlayingTrack
        {
            get { return this.queueManager.CurrentTrack().Value; }
        }

        public string PlayingTrackGuid
        {
            get { return this.queueManager.CurrentTrack().Key; }
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

                if (this.player != null && !this.mute) this.player.SetVolume(value);

                SettingsClient.Set<double>("Playback", "Volume", Math.Round(value, 2));
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
                await this.queueManager.ShuffleAsync();
            }
            else
            {
                await this.queueManager.UnShuffleAsync();

            }

            this.PlaybackShuffleChanged(this, new EventArgs());
            this.QueueChanged(this, new EventArgs());
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
                // Check if there is a Track playing
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
                // Check if there is a Track playing

                if (this.player != null && this.player.CanStop && this.PlayingTrack != null && this.PlayingTrack.Duration != null)
                {
                    // In some cases, the duration reported by TagLib is 1 second longer than the duration reported by CSCore.
                    if (this.PlayingTrack.Duration > this.player.GetTotalTime().TotalMilliseconds)
                    {
                        // To show the same duration everywhere, we report the TagLib duration here instead of the CSCore duration.
                        return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.PlayingTrack.Duration));
                    }
                    else
                    {
                        // Unless the TagLib duration is incorrect. In rare cases it is 0, even if 
                        // CSCore reports a correct duration. In such cases, report the CSCore duration.
                        return this.player.GetTotalTime();
                    }
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
        public PlaybackService(ITrackRepository trackRepository, ITrackStatisticRepository trackStatisticRepository, IEqualizerService equalizerService)
        {
            this.trackRepository = trackRepository;
            this.trackStatisticRepository = trackStatisticRepository;
            this.equalizerService = equalizerService;

            this.context = SynchronizationContext.Current;

            this.queueManager = new QueueManager();

            // Set up timers
            this.progressTimer.Interval = TimeSpan.FromSeconds(this.progressTimeoutSeconds).TotalMilliseconds;
            this.progressTimer.Elapsed += new ElapsedEventHandler(this.ProgressTimeoutHandler);

            this.savePlaybackCountersTimer.Interval = TimeSpan.FromSeconds(this.savePlaybackCountersTimeoutSeconds).TotalMilliseconds;
            this.savePlaybackCountersTimer.Elapsed += new ElapsedEventHandler(this.SavePlaybackCountersHandler);

            this.Initialize();
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
        public event Action<int> AddedTracksToQueue = delegate { };
        public event EventHandler PlaybackCountersChanged = delegate { };
        public event Action<bool> LoadingTrack = delegate { };
        public event EventHandler PlayingTrackPlaybackInfoChanged = delegate { };
        public event EventHandler PlayingTrackArtworkChanged = delegate { };
        public event EventHandler QueueChanged = delegate { };
        #endregion

        #region IPlaybackService
        public async Task StopIfPlayingAsync(PlayableTrack track)
        {
            if (track.Equals(this.PlayingTrack))
            {
                if (this.Queue.Count == 1)
                    this.Stop();
                else
                    await this.PlayNextAsync();
            }
        }

        public async Task UpdateQueueOrderAsync(List<PlayableTrack> tracks)
        {
            if (await this.queueManager.UpdateQueueOrderAsync(tracks, this.shuffle))
            {
                this.QueueChanged(this, new EventArgs()); // Required to update other Now Playing screens
            }
        }

        public async Task UpdateQueueMetadataAsync(List<FileMetadata> fileMetadatas)
        {
            UpdateQueueMetadataResult result = await this.queueManager.UpdateQueueMetadataAsync(fileMetadatas);

            // Raise events
            if (result.IsPlayingTrackPlaybackInfoChanged) this.PlayingTrackPlaybackInfoChanged(this, new EventArgs());
            if (result.IsPlayingTrackArtworkChanged) this.PlayingTrackArtworkChanged(this, new EventArgs());
            if (result.IsQueueChanged) this.QueueChanged(this, new EventArgs());
        }

        public async Task SetIsEqualizerEnabledAsync(bool isEnabled)
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

        public async Task SavePlaybackCountersAsync()
        {
            if (this.playbackCounters.Count == 0 | this.isSavingPlaybackCounters) return;

            this.savePlaybackCountersTimer.Stop();

            this.isSavingPlaybackCounters = true;

            IList<PlaybackCounter> counters = null;

            await Task.Run(() =>
            {
                lock (this.playbackCountersLock)
                {
                    counters = new List<PlaybackCounter>(this.playbackCounters.Values);
                    this.playbackCounters.Clear();
                }
            });

            foreach (PlaybackCounter counter in counters)
            {
                await this.trackStatisticRepository.UpdateCountersAsync(counter.Path, counter.PlayCount, counter.SkipCount, counter.DateLastPlayed);
            }

            this.PlaybackCountersChanged(this, new EventArgs());

            LogClient.Info("Saved playback counters");

            this.isSavingPlaybackCounters = false;

            // If, in the meantime, new counters are available, reset the timer.
            if (this.playbackCounters.Count > 0)
            {
                this.ResetSavePlaybackCountersTimer();
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
                if (this.Queue != null && this.Queue.Count > 0)
                {
                    // There are already tracks enqueued. Start playing immediately.
                    await this.PlayFirstAsync();
                }
                else
                {
                    // Enqueue all tracks before playing
                    await this.Enqueue();
                }
            }
        }

        public void SetMute(bool mute)
        {
            this.mute = mute;

            if (this.player != null)
            {
                this.player.SetVolume(mute ? 0.0f : this.Volume);
            }

            SettingsClient.Set<bool>("Playback", "Mute", this.mute);
            this.PlaybackMuteChanged(this, new EventArgs());
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
                if (this.PlayingTrack != null)
                {
                    int currentTime = this.GetCurrentTime.Seconds;
                    int totalTime = this.GetTotalTime.Seconds;

                    if (currentTime <= 10)
                    {
                        await this.UpdatePlaybackCountersAsync(this.PlayingTrack.Path, false, true); // Increase SkipCount
                    }
                    else
                    {
                        await this.UpdatePlaybackCountersAsync(this.PlayingTrack.Path, true, false); // Increase PlayCount
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get time information for Track with path='{0}'. Exception: {1}", this.PlayingTrack.Path, ex.Message);
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

        public async Task ShuffleAllAsync()
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(), TrackOrder.ByAlbum);

            if (await this.queueManager.ClearQueueAsync()) await this.queueManager.EnqueueAsync(tracks, this.shuffle);
            if (!this.shuffle) await SetShuffle(true); // Make sure tracks get shuffled
            await this.PlayFirstAsync();
        }

        public async Task Enqueue()
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(List<PlayableTrack> tracks)
        {
            if (tracks == null) return;

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(List<PlayableTrack> tracks, PlayableTrack track)
        {
            if (tracks == null || track == null) return;

            await this.EnqueueIfRequired(tracks);
            await this.PlaySelectedAsync(track);
        }

        public async Task Enqueue(Artist artist)
        {
            if (artist == null) return;

            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artist.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Genre genre)
        {
            if (genre == null) return;

            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genre.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Album album)
        {
            if (album == null) return;

            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(album.ToList()), TrackOrder.ByAlbum);

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task Enqueue(Playlist playlist)
        {
            if (playlist == null) return;

            // No ordering needed for playlists. Just enqueue tracks in the order that they are in the playlist.
            List<PlayableTrack> tracks = await this.trackRepository.GetTracksAsync(playlist.ToList());

            await this.EnqueueIfRequired(tracks);
            await this.PlayFirstAsync();
        }

        public async Task PlaySelectedAsync(PlayableTrack track)
        {
            await this.TryPlayAsync(new KeyValuePair<string, PlayableTrack>( null, track));
        }

        public async Task PlaySelectedAsync(KeyValuePair<string, PlayableTrack> track)
        {
            await this.TryPlayAsync(track);
        }

        public async Task<DequeueResult> Dequeue(IList<PlayableTrack> tracks)
        {
            DequeueResult dequeueResult = await this.queueManager.DequeueAsync(tracks);

            if (dequeueResult.IsSuccess & dequeueResult.IsPlayingTrackDequeued)
            {
                if (!dequeueResult.NextAvailableTrack.Equals(default(KeyValuePair<string,PlayableTrack>)))
                {
                    await this.TryPlayAsync(dequeueResult.NextAvailableTrack);
                }
                else
                {
                    this.Stop();
                }
            }

            this.QueueChanged(this, new EventArgs());

            return dequeueResult;
        }

        public async Task<EnqueueResult> AddToQueue(IList<PlayableTrack> tracks)
        {
            EnqueueResult result = await this.queueManager.EnqueueAsync(tracks, this.shuffle);

            this.QueueChanged(this, new EventArgs());

            if (result.EnqueuedTracks != null && result.IsSuccess)
            {
                this.AddedTracksToQueue(result.EnqueuedTracks.Count);
            }

            return result;
        }

        public async Task<EnqueueResult> AddToQueueNext(IList<PlayableTrack> tracks)
        {
            EnqueueResult result = await this.queueManager.EnqueueNextAsync(tracks, this.shuffle);

            this.QueueChanged(this, new EventArgs());

            if (result.EnqueuedTracks != null && result.IsSuccess)
            {
                this.AddedTracksToQueue(result.EnqueuedTracks.Count);
            }

            return result;
        }

        public async Task<EnqueueResult> AddToQueue(IList<Artist> artists)
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            return await this.AddToQueue(tracks);
        }

        public async Task<EnqueueResult> AddToQueue(IList<Genre> genres)
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            return await this.AddToQueue(tracks);
        }

        public async Task<EnqueueResult> AddToQueue(IList<Album> albums)
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            return await this.AddToQueue(tracks);
        }

        public async Task<EnqueueResult> AddToQueue(IList<Playlist> playlists)
        {
            List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(playlists), TrackOrder.ByAlbum);
            return await this.AddToQueue(tracks);
        }
        #endregion

        #region Private
        private async void Initialize()
        {
            // PlayerFactory
            this.playerFactory = new PlayerFactory();

            // Settings
            this.SetPlaybackSettings();

            // Equalizer
            await this.SetIsEqualizerEnabledAsync(SettingsClient.Get<bool>("Equalizer", "IsEnabled"));
        }

        private async void SavePlaybackCountersHandler(object sender, ElapsedEventArgs e)
        {
            await this.SavePlaybackCountersAsync();
        }

        private async Task UpdatePlaybackCountersAsync(string path, bool incrementPlayCount, bool incrementSkipCount)
        {
            await Task.Run(() =>
            {
                lock (this.playbackCountersLock)
                {
                    try
                    {
                        if (!this.playbackCounters.ContainsKey(path))
                        {
                            this.playbackCounters.Add(path, new PlaybackCounter { Path = path });
                        }

                        if (incrementPlayCount)
                        {
                            this.playbackCounters[path].PlayCount += 1;
                            this.playbackCounters[path].DateLastPlayed = DateTime.Now.Ticks;
                        }
                        if (incrementSkipCount) this.playbackCounters[path].SkipCount += 1;
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not update playback counters for track with path='{0}'. Exception: {1}", path, ex.Message);
                    }
                }
            });

            this.ResetSavePlaybackCountersTimer();
        }

        private async Task PauseAsync()
        {
            try
            {
                if (this.player != null)
                {
                    await Task.Run(() => this.player.Pause());
                    this.PlaybackPaused(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not pause track with path='{0}'. Exception: {1}", this.PlayingTrack.Path, ex.Message);
            }
        }

        private async Task ResumeAsync()
        {
            try
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
            catch (Exception ex)
            {
                LogClient.Error("Could not resume track with path='{0}'. Exception: {1}", this.PlayingTrack.Path, ex.Message);
            }
        }

        private async Task PlayFirstAsync()
        {
            if (this.Queue.Count > 0) await this.TryPlayAsync(this.queueManager.FirstTrack());
        }

        private void StopPlayback()
        {
            if (this.player != null)
            {
                // Remove the previous Stopped handler (not sure this is needed)
                this.player.PlaybackInterrupted -= this.PlaybackInterruptedHandler;
                this.player.PlaybackFinished -= this.PlaybackFinishedHandler;

                this.player.Stop();
                this.player.Dispose();
                this.player = null;
            }
        }

        private async Task StartPlaybackAsync(KeyValuePair<string, PlayableTrack> track, bool silent = false)
        {
            // Settings
            this.SetPlaybackSettings();

            // Play the Track from its runtime path (current or temporary)
            this.player = this.playerFactory.Create(Path.GetExtension(track.Value.Path));

            this.player.SetOutputDevice(this.Latency, this.EventMode, this.ExclusiveMode, this.activePreset.Bands);
            this.player.SetVolume(silent | this.Mute ? 0.0f : this.Volume);

            // We need to set PlayingTrack before trying to play the Track.
            // So if we go into the Catch when trying to play the Track,
            // at least, the next time TryPlayNext is called, it will know that 
            // we already tried to play this track and it can find the next Track.
            this.queueManager.SetCurrentTrack(track);

            // Play the Track
            await Task.Run(() => this.player.Play(track.Value.Path));

            // Start reporting progress
            this.progressTimer.Start();

            // Hook up the Stopped event
            this.player.PlaybackInterrupted += this.PlaybackInterruptedHandler;
            this.player.PlaybackFinished += this.PlaybackFinishedHandler;
        }

        private async Task<bool> TryPlayAsync(KeyValuePair<string,PlayableTrack> track, bool silent = false)
        {
            if (track.Value == null) return false;
            if (this.isLoadingTrack) return true; // Only load 1 track at a time (just in case)
            this.OnLoadingTrack(true);

            bool isPlaybackSuccess = true;
            PlaybackFailedEventArgs playbackFailedEventArgs = null;

            try
            {
                // If a Track was playing, make sure it is now stopped.
                this.StopPlayback();

                // Check that the file exists
                if (!System.IO.File.Exists(track.Value.Path))
                {
                    throw new FileNotFoundException(string.Format("File '{0}' was not found", track.Value.Path));
                }

                // Start playing
                await this.StartPlaybackAsync(track, silent);

                // Playing was successful
                this.PlaybackSuccess(this.isPlayingPreviousTrack);

                // Set this to false again after raising the event. It is important to have a correct slide 
                // direction for cover art when the next Track is a file from double click in Windows.
                this.isPlayingPreviousTrack = false;
                LogClient.Info("Playing the file {0}. EventMode={1}, ExclusiveMode={2}, LoopMode={3}, Shuffle={4}", track.Value.Path, this.eventMode.ToString(), this.exclusiveMode.ToString(), this.LoopMode.ToString(), this.shuffle.ToString());
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
                    LogClient.Error("Could not stop the Player");
                }

                LogClient.Error("Could not play the file {0}. EventMode={1}, ExclusiveMode={2}, LoopMode={3}, Shuffle={4}. Exception: {5}. StackTrace: {6}", track.Value.Path, this.eventMode.ToString(), this.exclusiveMode.ToString(), this.LoopMode.ToString(), this.shuffle.ToString(), playbackFailedEventArgs.Message, playbackFailedEventArgs.StackTrace);

                this.PlaybackFailed(this, playbackFailedEventArgs);
            }

            this.OnLoadingTrack(false);

            return isPlaybackSuccess;
        }

        private void OnLoadingTrack(bool isLoadingTrack)
        {
            this.isLoadingTrack = isLoadingTrack;
            this.LoadingTrack(isLoadingTrack);
        }

        private async Task<bool> TryPlayPreviousAsync()
        {
            this.isPlayingPreviousTrack = true;

            if (this.GetCurrentTime.Seconds > 3)
            {
                // If we're more than 3 seconds into the Track, try to
                // jump to the beginning of the current Track.
                this.player.Skip(0);
                return true;
            }

            KeyValuePair<string, PlayableTrack> previousTrack = await this.queueManager.PreviousTrackAsync(this.LoopMode);

            if (previousTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
            {
                this.Stop();
                return true;
            }

            return await this.TryPlayAsync(previousTrack);
        }

        private async Task<bool> TryPlayNextAsync()
        {
            this.isPlayingPreviousTrack = false;

            KeyValuePair<string, PlayableTrack> nextTrack = await this.queueManager.NextTrackAsync(this.LoopMode);

            if (nextTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
            {
                this.Stop();
                return true;
            }

            return await this.TryPlayAsync(nextTrack);
        }

        private void ProgressTimeoutHandler(object sender, ElapsedEventArgs e)
        {
            this.HandleProgress();
        }

        private void PlaybackInterruptedHandler(Object sender, PlaybackInterruptedEventArgs e)
        {
            // Playback was interrupted for some reason. Make sure we are in a correct state.
            // Use our context to trigger the work, because this event is fired on the Player's Playback thread.
            this.context.Post(new SendOrPostCallback((state) => this.Stop()), null);
        }

        private void PlaybackFinishedHandler(Object sender, EventArgs e)
        {
            // Try to play the next Track from the list automatically
            // Use our context to trigger the work, because this event is fired on the Player's Playback thread.
            this.context.Post(new SendOrPostCallback(async (state) =>
            {
                await this.UpdatePlaybackCountersAsync(this.PlayingTrack.Path, true, false); // Increase PlayCount
                await this.TryPlayNextAsync();
            }), null);
        }

        private async Task StartTrackPausedAsync(PlayableTrack track, int progressSeconds)
        {
            if (await this.TryPlayAsync(new KeyValuePair<string, PlayableTrack>(null, track), true))
            {
                await this.PauseAsync();
                if (!this.mute) this.player.SetVolume(this.Volume);
                this.player.Skip(progressSeconds);
                PlaybackProgressChanged(this, new EventArgs());
            }
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

            PlaybackProgressChanged(this, new EventArgs());
        }

        private async Task EnqueueIfRequired(List<PlayableTrack> tracks)
        {
            if (await this.queueManager.IsQueueDifferentAsync(tracks))
            {
                if (await this.queueManager.ClearQueueAsync())
                {
                    await this.queueManager.EnqueueAsync(tracks, this.shuffle);
                }

                this.QueueChanged(this, new EventArgs());
            }
        }

        private void ResetSavePlaybackCountersTimer()
        {
            this.savePlaybackCountersTimer.Stop();
            this.savePlaybackCountersTimer.Start();
        }

        private void SetPlaybackSettings()
        {
            this.LoopMode = (LoopMode)SettingsClient.Get<int>("Playback", "LoopMode");
            this.Latency = SettingsClient.Get<int>("Playback", "AudioLatency");
            this.Volume = SettingsClient.Get<float>("Playback", "Volume");
            this.mute = SettingsClient.Get<bool>("Playback", "Mute");
            this.shuffle = SettingsClient.Get<bool>("Playback", "Shuffle");
            this.EventMode = false;
            //this.EventMode = SettingsClient.Get<bool>("Playback", "WasapiEventMode");
            //this.ExclusiveMode = false;
            this.ExclusiveMode = SettingsClient.Get<bool>("Playback", "WasapiExclusiveMode");
        }
        #endregion
    }
}
