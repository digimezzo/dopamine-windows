using CSCore.CoreAudioAPI;
using Dopamine.Common.Audio;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Helpers;
using Dopamine.Common.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playback
{
    public delegate void PlaybackFailedEventHandler(object sender, PlaybackFailedEventArgs e);
    public delegate void PlaybackSuccessEventHandler(object sender, PlaybackSuccessEventArgs e);
    public delegate void PlaybackPausedEventHandler(object sender, PlaybackPausedEventArgs e);
    public delegate void TrackStatisticsChangedEventHandler(IList<TrackStatistic> statistics);

    public interface IPlaybackService
    {
        IPlayer Player { get; }
        KeyValuePair<string, PlayableTrack> CurrentTrack { get; }
        bool HasCurrentTrack { get; }
        bool IsSavingQueuedTracks { get; }
        bool IsSavingPlaybackCounters { get; }
        OrderedDictionary<string, PlayableTrack> Queue { get; }
        bool Shuffle { get; }
        bool Mute { get; }
        bool IsStopped { get; }
        bool IsPlaying { get; }
        TimeSpan GetCurrentTime { get; }
        TimeSpan GetTotalTime { get; }

        double Progress { get; set; }
        float Volume { get; set; }
        LoopMode LoopMode { get; set; }
        int Latency { get; set; }
        bool EventMode { get; set; }
        bool ExclusiveMode { get; set; }
        bool IsSpectrumVisible { get; set; }

        void Stop();
        void SkipProgress(double progress);
        void SkipSeconds(int jumpSeconds);
        void SetMute(bool mute);
        Task SetShuffleAsync(bool shuffle);
        Task PlayNextAsync();
        Task PlayPreviousAsync();
        Task PlayOrPauseAsync();
        Task PlaySelectedAsync(PlayableTrack track);
        Task PlaySelectedAsync(KeyValuePair<string, PlayableTrack> trackPair);
        Task<bool> PlaySelectedAsync(IList<PlayableTrack> tracks);
        Task EnqueueAsync(List<KeyValuePair<string, PlayableTrack>> trackPairs, KeyValuePair<string, PlayableTrack> track);
        Task EnqueueAsync(List<PlayableTrack> tracks, PlayableTrack trackPair);
        Task EnqueueAsync(List<PlayableTrack> tracks);
        Task EnqueueAsync(bool shuffle, bool unshuffle);
        Task EnqueueAsync(List<PlayableTrack> tracks, bool shuffle, bool unshuffle);
        Task EnqueueAsync(IList<Artist> artists, bool shuffle, bool unshuffle);
        Task EnqueueAsync(IList<Genre> genres, bool shuffle, bool unshuffle);
        Task EnqueueAsync(IList<Album> albums, bool shuffle, bool unshuffle);
        Task StopIfPlayingAsync(PlayableTrack track);
        Task<EnqueueResult> AddToQueueAsync(IList<PlayableTrack> tracks);
        Task<EnqueueResult> AddToQueueAsync(IList<Artist> artists);
        Task<EnqueueResult> AddToQueueAsync(IList<Genre> genres);
        Task<EnqueueResult> AddToQueueAsync(IList<Album> albums);
        Task<EnqueueResult> AddToQueueNextAsync(IList<PlayableTrack> tracks);
        Task<DequeueResult> DequeueAsync(IList<PlayableTrack> tracks);
        Task<DequeueResult> DequeueAsync(IList<KeyValuePair<string, PlayableTrack>> tracks);
        Task RefreshQueueLanguageAsync();
        Task SaveQueuedTracksAsync();
        Task SavePlaybackCountersAsync();
        void ApplyPreset(EqualizerPreset preset);
        Task SetIsEqualizerEnabledAsync(bool isEnabled);
        Task UpdateQueueMetadataAsync(List<FileMetadata> fileMetadatas);
        Task UpdateQueueOrderAsync(List<KeyValuePair<string, PlayableTrack>> tracks);
        Task<IList<MMDevice>> GetAllOutputDevicesAsync();
        Task SwitchOutputDeviceAsync(MMDevice outputDevice);
        Task<MMDevice> GetSavedAudioDeviceAsync();

        event PlaybackSuccessEventHandler PlaybackSuccess;
        event PlaybackFailedEventHandler PlaybackFailed;
        event PlaybackPausedEventHandler PlaybackPaused;
        event EventHandler PlaybackSkipped;
        event EventHandler PlaybackStopped;
        event EventHandler PlaybackResumed;
        event EventHandler PlaybackProgressChanged;
        event EventHandler PlaybackVolumeChanged;
        event EventHandler PlaybackMuteChanged;
        event EventHandler PlaybackLoopChanged;
        event EventHandler PlaybackShuffleChanged;
        event Action<bool> SpectrumVisibilityChanged;
        event Action<int> AddedTracksToQueue;
        event TrackStatisticsChangedEventHandler TrackStatisticsChanged;
        event Action<bool> LoadingTrack;
        event EventHandler PlayingTrackPlaybackInfoChanged;
        event EventHandler PlayingTrackArtworkChanged;
        event EventHandler QueueChanged;
        event EventHandler AudioDevicesChanged;
    }
}
