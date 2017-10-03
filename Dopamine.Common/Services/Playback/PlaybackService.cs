using CSCore.CoreAudioAPI;
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
using Dopamine.Common.Services.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Services.I18n;

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

        private bool isQueueChanged;

        private II18nService i18nService;
        private IFileService fileService;
        private IEqualizerService equalizerService;
        private EqualizerPreset desiredPreset;
        private EqualizerPreset activePreset;
        private bool isEqualizerEnabled;

        private IQueuedTrackRepository queuedTrackRepository;
        private System.Timers.Timer saveQueuedTracksTimer = new System.Timers.Timer();
        private int saveQueuedTracksTimeoutSeconds = 5;

        private bool isSavingQueuedTracks = false;

        private IPlayerFactory playerFactory;

        private ITrackRepository trackRepository;
        private ITrackStatisticRepository trackStatisticRepository;

        private System.Timers.Timer saveTrackStatisticsTimer = new System.Timers.Timer();
        private int savePlaybackCountersTimeoutSeconds = 5;

        private bool isSavingTrackStatistics = false;
        private Dictionary<string, TrackStatistic> trackStatistics = new Dictionary<string, TrackStatistic>();

        private object trackStatisticsLock = new object();

        private SynchronizationContext context;
        private bool isLoadingTrack;

        private MMDevice outputDevice;
        private AudioDevicesWatcher watcher = new AudioDevicesWatcher();
        #endregion

        #region Properties
        public bool IsSavingQueuedTracks
        {
            get { return this.isSavingQueuedTracks; }
        }

        public bool IsSavingPlaybackCounters
        {
            get { return this.isSavingTrackStatistics; }
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

        public KeyValuePair<string, PlayableTrack> CurrentTrack
        {
            get { return this.queueManager.CurrentTrack(); }
        }

        public bool HasCurrentTrack
        {
            get { return !this.queueManager.CurrentTrack().Equals(default(KeyValuePair<string, PlayableTrack>)) && this.queueManager.CurrentTrack().Value != null; }
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

        public async Task SetShuffleAsync(bool isShuffled)
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

                if (this.player != null && this.player.CanStop && this.HasCurrentTrack && this.CurrentTrack.Value.Duration != null)
                {
                    // In some cases, the duration reported by TagLib is 1 second longer than the duration reported by CSCore.
                    if (this.CurrentTrack.Value.Duration > this.player.GetTotalTime().TotalMilliseconds)
                    {
                        // To show the same duration everywhere, we report the TagLib duration here instead of the CSCore duration.
                        return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.CurrentTrack.Value.Duration));
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
        public PlaybackService(IFileService fileService, II18nService i18nService, ITrackRepository trackRepository, ITrackStatisticRepository trackStatisticRepository, IEqualizerService equalizerService, IQueuedTrackRepository queuedTrackRepository)
        {
            this.fileService = fileService;
            this.i18nService = i18nService;
            this.trackRepository = trackRepository;
            this.trackStatisticRepository = trackStatisticRepository;
            this.queuedTrackRepository = queuedTrackRepository;
            this.equalizerService = equalizerService;

            this.context = SynchronizationContext.Current;

            this.queueManager = new QueueManager();

            // Event handlers
            this.fileService.TracksImported += async (tracks) => await this.EnqueueAsync(tracks);
            this.i18nService.LanguageChanged += (_, __) => this.RefreshQueueLanguageAsync();

            // Set up timers
            this.progressTimer.Interval = TimeSpan.FromSeconds(this.progressTimeoutSeconds).TotalMilliseconds;
            this.progressTimer.Elapsed += new ElapsedEventHandler(this.ProgressTimeoutHandler);

            this.saveQueuedTracksTimer.Interval = TimeSpan.FromSeconds(this.saveQueuedTracksTimeoutSeconds).TotalMilliseconds;
            this.saveQueuedTracksTimer.Elapsed += new ElapsedEventHandler(this.SaveQueuedTracksTimeoutHandler);

            this.saveTrackStatisticsTimer.Interval = TimeSpan.FromSeconds(this.savePlaybackCountersTimeoutSeconds).TotalMilliseconds;
            this.saveTrackStatisticsTimer.Elapsed += new ElapsedEventHandler(this.SavePlaybackCountersHandler);

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
        public event TrackStatisticsChangedEventHandler TrackStatisticsChanged = delegate { };
        public event Action<bool> LoadingTrack = delegate { };
        public event EventHandler PlayingTrackPlaybackInfoChanged = delegate { };
        public event EventHandler PlayingTrackArtworkChanged = delegate { };
        public event EventHandler QueueChanged = delegate { };
        public event EventHandler AudioDevicesChanged = delegate { };
        public event EventHandler PlaybackSkipped = delegate { };
        #endregion

        #region IPlaybackService
        public async Task<MMDevice> GetSavedAudioDeviceAsync()
        {
            string savedAudioDeviceID = SettingsClient.Get<string>("Playback", "AudioDevice");

            IList<MMDevice> outputDevices = await this.GetAllOutputDevicesAsync();
            MMDevice savedDevice = outputDevices.Select(d => d).Where(d => d != null && d.DeviceID.Equals(savedAudioDeviceID)).FirstOrDefault();

            return savedDevice;
        }

        public async Task<IList<MMDevice>> GetAllOutputDevicesAsync()
        {
            List<MMDevice> devices = null;
            await Task.Run(() =>
            {
                devices = new List<MMDevice>();
                using (var mmdeviceEnumerator = new MMDeviceEnumerator())
                {
                    using (
                        var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                    {
                        foreach (var device in mmdeviceCollection)
                        {
                            devices.Add(device);
                        }
                    }
                }
            });

            return devices;
        }

        public async Task SwitchOutputDeviceAsync(MMDevice device)
        {
            if (device != null && this.outputDevice != null && device.DeviceID == this.outputDevice.DeviceID) return;

            this.outputDevice = device;

            await Task.Run(() =>
            {
                if (this.player != null)
                {
                    this.player.SwitchOutputDevice(this.outputDevice);
                }
            });
        }

        public async Task StopIfPlayingAsync(PlayableTrack track)
        {
            if (track.Equals(this.CurrentTrack.Value))
            {
                if (this.Queue.Count == 1)
                    this.Stop();
                else
                    await this.PlayNextAsync();
            }
        }

        public async Task UpdateQueueOrderAsync(List<KeyValuePair<string, PlayableTrack>> tracks)
        {
            if (await this.queueManager.UpdateQueueOrderAsync(tracks, this.shuffle))
            {
                this.QueueChanged(this, new EventArgs()); // Required to update other Now Playing screens
            }
        }

        public async Task UpdateQueueMetadataAsync(List<FileMetadata> fileMetadatas)
        {
            UpdateQueueMetadataResult result = await this.queueManager.UpdateMetadataAsync(fileMetadatas);

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

        public async Task SaveQueuedTracksAsync()
        {
            if (!this.isQueueChanged) return;

            this.saveQueuedTracksTimer.Stop();
            this.isSavingQueuedTracks = true;

            try
            {
                var queuedTracks = new List<QueuedTrack>();
                OrderedDictionary<string, PlayableTrack> tracks = this.Queue;
                KeyValuePair<string, PlayableTrack> currentTrack = this.CurrentTrack;
                long progressSeconds = Convert.ToInt64(this.GetCurrentTime.TotalSeconds);

                int orderID = 0;

                foreach (KeyValuePair<string, PlayableTrack> track in tracks)
                {
                    var queuedTrack = new QueuedTrack();
                    queuedTrack.QueueID = track.Key;
                    queuedTrack.Path = track.Value.Path;
                    queuedTrack.SafePath = track.Value.SafePath;
                    queuedTrack.OrderID = orderID;
                    queuedTrack.IsPlaying = 0;
                    queuedTrack.ProgressSeconds = 0;

                    if (track.Key.Equals(currentTrack.Key))
                    {
                        queuedTrack.IsPlaying = 1;
                        queuedTrack.ProgressSeconds = progressSeconds;
                    }

                    queuedTracks.Add(queuedTrack);

                    orderID++;
                }

                await this.queuedTrackRepository.SaveQueuedTracksAsync(queuedTracks);

                LogClient.Info("Saved {0} queued tracks", queuedTracks.Count.ToString());
            }
            catch (Exception ex)
            {
                LogClient.Info("Could not save queued tracks. Exception: {0}", ex.Message);
            }

            this.isSavingQueuedTracks = false;
        }

        public async Task SavePlaybackCountersAsync()
        {
            if (this.trackStatistics.Count == 0 | this.isSavingTrackStatistics) return;

            this.saveTrackStatisticsTimer.Stop();

            this.isSavingTrackStatistics = true;

            IList<TrackStatistic> localTrackStatistics = null;

            await Task.Run(() =>
            {
                lock (this.trackStatisticsLock)
                {
                    localTrackStatistics = new List<TrackStatistic>(this.trackStatistics.Values);
                    this.trackStatistics.Clear();
                }
            });

            foreach (TrackStatistic localTrackStatistic in localTrackStatistics)
            {
                await this.trackStatisticRepository.UpdateTrackStatisticAsync(localTrackStatistic);
            }

            this.TrackStatisticsChanged(localTrackStatistics);

            LogClient.Info("Saved track statistics");

            this.isSavingTrackStatistics = false;

            // If, in the meantime, new track statistics are available, reset the timer.
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
                if (this.Queue != null && this.Queue.Count > 0)
                {
                    // There are already tracks enqueued. Start playing immediately.
                    await this.PlayFirstAsync();
                }
                else
                {
                    // Enqueue all tracks before playing
                    await this.EnqueueAsync(false, false);
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

        public void SkipProgress(double progress)
        {
            if (this.player != null && this.player.CanStop)
            {
                this.Progress = progress;
                int newSeconds = Convert.ToInt32(progress * this.player.GetTotalTime().TotalSeconds);
                this.player.Skip(newSeconds);
                this.PlaybackSkipped(this, new EventArgs());
            }
            else
            {
                this.Progress = 0.0;
            }

            this.PlaybackProgressChanged(this, new EventArgs());
        }

        public void SkipSeconds(int seconds)
        {
            if (this.player != null && this.player.CanStop)
            {
                this.player.Skip(Convert.ToInt32(this.GetCurrentTime.TotalSeconds + seconds));
                this.PlaybackSkipped(this, new EventArgs());
                this.PlaybackProgressChanged(this, new EventArgs());
            }
        }

        public void Stop()
        {
            if (this.player != null && this.player.CanStop)
            {
                this.player.Stop();
            }

            this.PlayingTrackArtworkChanged(this, new EventArgs());
            this.PlayingTrackPlaybackInfoChanged(this, new EventArgs());

            this.progressTimer.Stop();
            this.Progress = 0.0;
            this.PlaybackStopped(this, new EventArgs());
        }

        public async Task PlayNextAsync()
        {
            LogClient.Info("Request to play the next track.");

            if (this.HasCurrentTrack)
            {
                try
                {
                    int currentTime = this.GetCurrentTime.Seconds;
                    int totalTime = this.GetTotalTime.Seconds;

                    if (currentTime <= 10)
                    {
                        await this.UpdateTrackStatisticsAsync(this.CurrentTrack.Value.Path, false, true); // Increase SkipCount
                    }
                    else
                    {
                        await this.UpdateTrackStatisticsAsync(this.CurrentTrack.Value.Path, true, false); // Increase PlayCount
                    }

                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get time information for Track with path='{0}'. Exception: {1}", this.CurrentTrack.Value.Path, ex.Message);
                }
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
                    playSuccess = await this.TryPlayNextAsync(true);
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
            LogClient.Info("Request to play the previous track.");

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
                    playSuccess = await this.TryPlayPreviousAsync(true);
                }
                else
                {
                    this.Stop();
                    playSuccess = true; // Otherwise we never get out of this While loop
                }
            }
        }

        public async Task EnqueueAsync(List<PlayableTrack> tracks, bool shuffle, bool unshuffle)
        {
            if (tracks == null) return;

            // Shuffle
            if (shuffle) await this.EnqueueIfDifferent(tracks, true);

            // Unshuffle
            if (unshuffle) await this.EnqueueIfDifferent(tracks, false);

            // Use the current shuffle mode
            if (!shuffle && !unshuffle) await this.EnqueueIfDifferent(tracks, this.shuffle);

            // Start playing
            await this.PlayFirstAsync();
        }

        public async Task EnqueueAsync(bool shuffle, bool unshuffle)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(), TrackOrder.ByAlbum);
            await this.EnqueueAsync(tracks, shuffle, unshuffle);
        }

        public async Task EnqueueAsync(List<PlayableTrack> tracks)
        {
            await this.EnqueueAsync(tracks, false, false);
        }

        public async Task EnqueueAsync(List<PlayableTrack> tracks, PlayableTrack track)
        {
            if (tracks == null || track == null) return;

            await this.EnqueueIfDifferent(tracks, this.shuffle);
            await this.PlaySelectedAsync(track);
        }

        public async Task EnqueueAsync(List<KeyValuePair<string, PlayableTrack>> trackPairs, KeyValuePair<string, PlayableTrack> trackPair)
        {
            if (trackPairs == null || trackPair.Value == null) return;

            await this.EnqueueIfRequired(trackPairs);
            await this.PlaySelectedAsync(trackPair);
        }

        public async Task EnqueueAsync(IList<Artist> artists, bool shuffle, bool unshuffle)
        {
            if (artists == null) return;

            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            await this.EnqueueAsync(tracks, shuffle, unshuffle);
        }

        public async Task EnqueueAsync(IList<Genre> genres, bool shuffle, bool unshuffle)
        {
            if (genres == null) return;

            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            await this.EnqueueAsync(tracks, shuffle, unshuffle);
        }

        public async Task EnqueueAsync(IList<Album> albums, bool shuffle, bool unshuffle)
        {
            if (albums == null) return;

            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            await this.EnqueueAsync(tracks, shuffle, unshuffle);
        }

        public async Task PlaySelectedAsync(PlayableTrack track)
        {
            await this.TryPlayAsync(new KeyValuePair<string, PlayableTrack>(null, track));
        }

        public async Task PlaySelectedAsync(KeyValuePair<string, PlayableTrack> track)
        {
            await this.TryPlayAsync(track);
        }

        public async Task<bool> PlaySelectedAsync(IList<PlayableTrack> tracks)
        {
            var result = await this.queueManager.ClearQueueAsync();
            if (result)
            {
                result = (await this.AddToQueueAsync(tracks)).IsSuccess;
                if (result)
                    await this.PlayNextAsync();
            }

            return result;
        }

        public async Task<DequeueResult> DequeueAsync(IList<PlayableTrack> tracks)
        {
            IList<KeyValuePair<string, PlayableTrack>> trackPairs = new List<KeyValuePair<string, PlayableTrack>>();

            await Task.Run(() =>
            {
                foreach (PlayableTrack track in tracks)
                {
                    // New Guids are created here, they will never be found in the queue.
                    // QueueManager will dequeue all tracks which have a matching SafePath.
                    trackPairs.Add(new KeyValuePair<string, PlayableTrack>(Guid.NewGuid().ToString(), track));
                }
            });

            DequeueResult dequeueResult = await this.queueManager.DequeueAsync(trackPairs);

            if (dequeueResult.IsSuccess & dequeueResult.IsPlayingTrackDequeued)
            {
                if (!dequeueResult.NextAvailableTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
                {
                    await this.TryPlayAsync(dequeueResult.NextAvailableTrack);
                }
                else
                {
                    this.Stop();
                }
            }

            this.QueueChanged(this, new EventArgs());

            this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database

            return dequeueResult;
        }

        public async Task<DequeueResult> DequeueAsync(IList<KeyValuePair<string, PlayableTrack>> tracks)
        {
            DequeueResult dequeueResult = await this.queueManager.DequeueAsync(tracks);

            if (dequeueResult.IsSuccess & dequeueResult.IsPlayingTrackDequeued)
            {
                if (!dequeueResult.NextAvailableTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
                {
                    await this.TryPlayAsync(dequeueResult.NextAvailableTrack);
                }
                else
                {
                    this.Stop();
                }
            }

            this.QueueChanged(this, new EventArgs());

            this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database

            return dequeueResult;
        }

        public async Task<EnqueueResult> AddToQueueAsync(IList<PlayableTrack> tracks)
        {
            EnqueueResult result = await this.queueManager.EnqueueAsync(tracks, this.shuffle);

            this.QueueChanged(this, new EventArgs());

            if (result.EnqueuedTracks != null && result.IsSuccess)
            {
                this.AddedTracksToQueue(result.EnqueuedTracks.Count);
            }

            this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database

            return result;
        }

        public async Task<EnqueueResult> AddToQueueNextAsync(IList<PlayableTrack> tracks)
        {
            EnqueueResult result = await this.queueManager.EnqueueNextAsync(tracks);

            this.QueueChanged(this, new EventArgs());

            if (result.EnqueuedTracks != null && result.IsSuccess)
            {
                this.AddedTracksToQueue(result.EnqueuedTracks.Count);
            }

            this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database

            return result;
        }

        public async Task<EnqueueResult> AddToQueueAsync(IList<Artist> artists)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            return await this.AddToQueueAsync(tracks);
        }

        public async Task<EnqueueResult> AddToQueueAsync(IList<Genre> genres)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            return await this.AddToQueueAsync(tracks);
        }

        public async Task<EnqueueResult> AddToQueueAsync(IList<Album> albums)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            return await this.AddToQueueAsync(tracks);
        }
        #endregion

        #region Private
        private void CheckWindowsMediaFoundationAsync()
        {
            try
            {
                if (!System.IO.File.Exists(Path.Combine(Environment.SystemDirectory, "mf.dll")))
                {
                    LogClient.Error("Windows Media Foundation could not be found.");
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while trying to locate Windows Media Foundation. Exception: {0}", ex.Message);
            }
        }

        private async void Initialize()
        {
            // Check if Windows Media Foundation is available
            this.CheckWindowsMediaFoundationAsync();

            // PlayerFactory
            this.playerFactory = new PlayerFactory();

            // Settings
            this.SetPlaybackSettings();

            // Audio device
            await this.SetAudioDeviceAsync();

            // Detect audio device changes
            this.watcher.StartWatching();
            this.watcher.AudioDevicesChanged += Watcher_AudioDevicesChanged;

            // Equalizer
            await this.SetIsEqualizerEnabledAsync(SettingsClient.Get<bool>("Equalizer", "IsEnabled"));

            // Queued tracks
            this.GetSavedQueuedTracks();
        }

        private async void SavePlaybackCountersHandler(object sender, ElapsedEventArgs e)
        {
            await this.SavePlaybackCountersAsync();
        }

        private async Task UpdateTrackStatisticsAsync(string path, bool incrementPlayCount, bool incrementSkipCount)
        {

            if (!this.trackStatistics.ContainsKey(path))
            {
                // Try to find existing statistic
                TrackStatistic trackStatistic = await this.trackStatisticRepository.GetTrackStatisticAsync(path);

                // If no existing statistic was found, create a new one.
                if (trackStatistic == null)
                {
                    trackStatistic = new TrackStatistic();
                }

                // Add statistic to the dictionary
                lock (this.trackStatisticsLock)
                {
                    this.trackStatistics.Add(path, trackStatistic);
                }
            }

            await Task.Run(() =>
            {
                lock (this.trackStatisticsLock)
                {
                    try
                    {
                        if (incrementPlayCount)
                        {
                            if (this.trackStatistics[path].PlayCount != null)
                            {
                                this.trackStatistics[path].PlayCount += 1;
                            }
                            else
                            {
                                this.trackStatistics[path].PlayCount = 1;
                            }

                            this.trackStatistics[path].DateLastPlayed = DateTime.Now.Ticks;
                        }
                        if (incrementSkipCount)
                        {
                            if (this.trackStatistics[path].SkipCount != null)
                            {
                                this.trackStatistics[path].SkipCount += 1;
                            }
                            else
                            {
                                this.trackStatistics[path].SkipCount = 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not update track statistics for track with path='{0}'. Exception: {1}", path, ex.Message);
                    }
                }
            });

            this.ResetSaveTrackStatisticsTimer();
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
                LogClient.Error("Could not pause track with path='{0}'. Exception: {1}", this.CurrentTrack.Value.Path, ex.Message);
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
                LogClient.Error("Could not resume track with path='{0}'. Exception: {1}", this.CurrentTrack.Value.Path, ex.Message);
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
            // If we start playing a track, we need to make sure that
            // queued tracks are saved when the application is closed.
            this.isQueueChanged = true;

            // Settings
            this.SetPlaybackSettings();

            // Play the Track from its runtime path (current or temporary)
            this.player = this.playerFactory.Create(Path.GetExtension(track.Value.Path));

            this.player.SetPlaybackSettings(this.Latency, this.EventMode, this.ExclusiveMode, this.activePreset.Bands);
            this.player.SetVolume(silent | this.Mute ? 0.0f : this.Volume);

            // We need to set PlayingTrack before trying to play the Track.
            // So if we go into the Catch when trying to play the Track,
            // at least, the next time TryPlayNext is called, it will know that 
            // we already tried to play this track and it can find the next Track.
            this.queueManager.SetCurrentTrack(track);

            // Play the Track
            await Task.Run(() => this.player.Play(track.Value.Path, this.outputDevice));

            // Start reporting progress
            this.progressTimer.Start();

            // Hook up the Stopped event
            this.player.PlaybackInterrupted += this.PlaybackInterruptedHandler;
            this.player.PlaybackFinished += this.PlaybackFinishedHandler;
        }

        private async Task<bool> TryPlayAsync(KeyValuePair<string, PlayableTrack> trackPair, bool silent = false)
        {
            if (trackPair.Value == null) return false;
            if (this.isLoadingTrack) return true; // Only load 1 track at a time (just in case)
            this.OnLoadingTrack(true);

            bool isPlaybackSuccess = true;
            PlaybackFailedEventArgs playbackFailedEventArgs = null;

            try
            {
                // If a Track was playing, make sure it is now stopped.
                this.StopPlayback();

                // Check that the file exists
                if (!System.IO.File.Exists(trackPair.Value.Path))
                {
                    throw new FileNotFoundException(string.Format("File '{0}' was not found", trackPair.Value.Path));
                }

                // Start playing
                await this.StartPlaybackAsync(trackPair, silent);

                // Playing was successful
                this.PlaybackSuccess(this.isPlayingPreviousTrack);

                // Set this to false again after raising the event. It is important to have a correct slide 
                // direction for cover art when the next Track is a file from double click in Windows.
                this.isPlayingPreviousTrack = false;
                LogClient.Info("Playing the file {0}. EventMode={1}, ExclusiveMode={2}, LoopMode={3}, Shuffle={4}", trackPair.Value.Path, this.eventMode.ToString(), this.exclusiveMode.ToString(), this.LoopMode.ToString(), this.shuffle.ToString());
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

                LogClient.Error("Could not play the file {0}. EventMode={1}, ExclusiveMode={2}, LoopMode={3}, Shuffle={4}. Exception: {5}. StackTrace: {6}", trackPair.Value.Path, this.eventMode.ToString(), this.exclusiveMode.ToString(), this.LoopMode.ToString(), this.shuffle.ToString(), playbackFailedEventArgs.Message, playbackFailedEventArgs.StackTrace);

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

        private async Task<bool> TryPlayPreviousAsync(bool ignoreLoopOne)
        {
            this.isPlayingPreviousTrack = true;

            if (this.GetCurrentTime.Seconds > 3)
            {
                // If we're more than 3 seconds into the Track, try to
                // jump to the beginning of the current Track.
                this.player.Skip(0);
                return true;
            }

            // When "loop one" is enabled and ignoreLoopOne is true, act like "loop all".
            LoopMode loopMode = this.LoopMode == LoopMode.One && ignoreLoopOne ? LoopMode.All : this.LoopMode;

            KeyValuePair<string, PlayableTrack> previousTrack = await this.queueManager.PreviousTrackAsync(loopMode);

            if (previousTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
            {
                this.Stop();
                return true;
            }

            return await this.TryPlayAsync(previousTrack);
        }

        private async Task<bool> TryPlayNextAsync(bool ignoreLoopOne)
        {
            this.isPlayingPreviousTrack = false;

            LoopMode loopMode = this.LoopMode == LoopMode.One && ignoreLoopOne ? LoopMode.All : this.LoopMode;

            // When "loop one" is enabled and ignoreLoopOne is true, act like "loop all".
            bool returnToStart = SettingsClient.Get<bool>("Playback", "LoopWhenShuffle") & this.shuffle;
            KeyValuePair<string, PlayableTrack> nextTrack = await this.queueManager.NextTrackAsync(loopMode, returnToStart);

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
            this.context.Post(new SendOrPostCallback((state) =>
            {
                LogClient.Info("Track interrupted: {0}", this.CurrentTrack.Value.Path);
                this.Stop();
            }), null);
        }

        private void PlaybackFinishedHandler(Object sender, EventArgs e)
        {
            // Try to play the next Track from the list automatically
            // Use our context to trigger the work, because this event is fired on the Player's Playback thread.
            this.context.Post(new SendOrPostCallback(async (state) =>
            {
                LogClient.Info("Track finished: {0}", this.CurrentTrack.Value.Path);
                await this.UpdateTrackStatisticsAsync(this.CurrentTrack.Value.Path, true, false); // Increase PlayCount
                await this.TryPlayNextAsync(false);
            }), null);
        }

        private async void SaveQueuedTracksTimeoutHandler(object sender, ElapsedEventArgs e)
        {
            await this.SaveQueuedTracksAsync();
        }

        private async void GetSavedQueuedTracks()
        {
            try
            {
                List<QueuedTrack> savedQueuedTracks = await this.queuedTrackRepository.GetSavedQueuedTracksAsync();
                List<PlayableTrack> tracks = await this.trackRepository.GetTracksAsync(savedQueuedTracks.Select(t => t.Path).ToList());
                var tracksDictionary = new Dictionary<string, PlayableTrack>();
                var tracksToEnqueue = new List<KeyValuePair<string, PlayableTrack>>();
                KeyValuePair<string, PlayableTrack> playingTrack = default(KeyValuePair<string, PlayableTrack>);
                int progressSeconds = 0;

                await Task.Run(() =>
                {
                    // Makes lookup faster
                    foreach (var track in tracks)
                    {
                        tracksDictionary.Add(track.SafePath, track);
                    }

                    foreach (QueuedTrack savedQueuedTrack in savedQueuedTracks)
                    {
                        KeyValuePair<string, PlayableTrack> trackToEnqueue = default(KeyValuePair<string, PlayableTrack>);

                        // Enqueue only tracks which are found in the database and exist on disk.
                        if (tracksDictionary.ContainsKey(savedQueuedTrack.SafePath) && System.IO.File.Exists(savedQueuedTrack.Path))
                        {
                            // Create the track
                            trackToEnqueue = new KeyValuePair<string, PlayableTrack>(savedQueuedTrack.QueueID, tracksDictionary[savedQueuedTrack.SafePath]);

                            // Check if the track was playing
                            if (savedQueuedTrack.IsPlaying == 1)
                            {
                                playingTrack = trackToEnqueue;
                                progressSeconds = Convert.ToInt32(savedQueuedTrack.ProgressSeconds);
                            }

                            // Add to tracksToEnqueue
                            tracksToEnqueue.Add(trackToEnqueue);
                        }
                    }
                });

                if (tracksToEnqueue.Count > 0)
                {
                    await this.queueManager.EnqueueAsync(tracksToEnqueue, this.shuffle);
                    this.QueueChanged(this, new EventArgs());
                }

                if (!SettingsClient.Get<bool>("Startup", "RememberLastPlayedTrack")) return;

                if (!playingTrack.Equals(default(KeyValuePair<string, PlayableTrack>)))
                {
                    try
                    {
                        await this.StartTrackPausedAsync(playingTrack, progressSeconds);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not set the playing track. Exception: {0}", ex.Message);
                        this.Stop(); // Should not be required, but just in case.
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get saved queued tracks. Exception: {0}", ex.Message);
            }
        }

        private async Task StartTrackPausedAsync(KeyValuePair<string, PlayableTrack> track, int progressSeconds)
        {
            if (await this.TryPlayAsync(track, true))
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

        private async Task EnqueueIfDifferent(List<PlayableTrack> tracks, bool shuffle)
        {
            if (await this.queueManager.IsQueueDifferentAsync(tracks) || shuffle != this.shuffle)
            {
                if (await this.queueManager.ClearQueueAsync())
                {
                    await this.queueManager.EnqueueAsync(tracks, shuffle);

                    if (shuffle != this.shuffle)
                    {
                        this.shuffle = shuffle;
                        this.PlaybackShuffleChanged(this, new EventArgs());
                    }
                }

                this.QueueChanged(this, new EventArgs());
                this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database
            }
        }

        private async Task EnqueueIfRequired(List<KeyValuePair<string, PlayableTrack>> tracks)
        {
            if (await this.queueManager.IsQueueDifferentAsync(tracks))
            {
                if (await this.queueManager.ClearQueueAsync())
                {
                    await this.queueManager.EnqueueAsync(tracks, this.shuffle);
                }

                this.QueueChanged(this, new EventArgs());

                this.ResetSaveQueuedTracksTimer(); // Save queued tracks to the database
            }
        }

        private void ResetSaveQueuedTracksTimer()
        {
            this.saveQueuedTracksTimer.Stop();
            this.isQueueChanged = true;
            this.saveQueuedTracksTimer.Start();
        }

        private void ResetSaveTrackStatisticsTimer()
        {
            this.saveTrackStatisticsTimer.Stop();
            this.saveTrackStatisticsTimer.Start();
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

        private async Task SetAudioDeviceAsync()
        {
            this.outputDevice = await this.GetSavedAudioDeviceAsync();
        }

        private async void Watcher_AudioDevicesChanged(object sender, EventArgs e)
        {
            await this.SetAudioDeviceAsync();
            this.AudioDevicesChanged(this, new EventArgs());
        }

        public async Task RefreshQueueLanguageAsync()
        {
            List<PlayableTrack> databaseTracks = await this.trackRepository.GetTracksAsync(this.Queue.Select(t => t.Value.Path).ToList());

            await Task.Run(() =>
            {
                foreach (KeyValuePair<string, PlayableTrack> pair in this.Queue)
                {
                    PlayableTrack databaseTrack = databaseTracks.Where(t => t.SafePath.Equals(pair.Value.SafePath)).FirstOrDefault();

                    if (databaseTrack != null)
                    {
                        pair.Value.ArtistName = databaseTrack.ArtistName;
                        pair.Value.AlbumArtist = databaseTrack.AlbumArtist;
                        pair.Value.AlbumTitle = databaseTrack.AlbumTitle;
                        pair.Value.GenreName = databaseTrack.GenreName;
                    }
                }
            });

            this.QueueChanged(this, new EventArgs());
            this.PlayingTrackPlaybackInfoChanged(this, new EventArgs());
        }
        #endregion
    }
}
