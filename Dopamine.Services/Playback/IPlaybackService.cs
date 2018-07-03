using CSCore.CoreAudioAPI;
using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playback
{
    public delegate void PlaybackFailedEventHandler(object sender, PlaybackFailedEventArgs e);
    public delegate void PlaybackSuccessEventHandler(object sender, PlaybackSuccessEventArgs e);
    public delegate void PlaybackPausedEventHandler(object sender, PlaybackPausedEventArgs e);
    public delegate void TrackStatisticsChangedEventHandler(IList<TrackStatistic> statistics);
    public delegate void PlaybackVolumeChangedEventhandler(object sender, PlaybackVolumeChangedEventArgs e);

    public interface IPlaybackService
    {
        IPlayer Player { get; }
        KeyValuePair<string, TrackViewModel> CurrentTrack { get; }

        bool SupportsWindowsMediaFoundation { get; }
        bool HasCurrentTrack { get; }
        bool IsSavingQueuedTracks { get; }
        bool IsSavingPlaybackCounters { get; }
        OrderedDictionary<string, TrackViewModel> Queue { get; }
        bool Shuffle { get; }
        bool Mute { get; }
        bool IsStopped { get; }
        bool IsPlaying { get; }
        TimeSpan GetCurrentTime { get; }
        TimeSpan GetTotalTime { get; }

        double Progress { get; set; }
        float Volume { get; set; }
        LoopMode LoopMode { get; set; }
        bool UseAllAvailableChannels { get; set; }
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
        Task PlaySelectedAsync(TrackViewModel track);
        Task PlaySelectedAsync(KeyValuePair<string, TrackViewModel> trackPair);
        Task<bool> PlaySelectedAsync(IList<TrackViewModel> tracks);
        Task EnqueueAsync(List<KeyValuePair<string, TrackViewModel>> trackPairs, KeyValuePair<string, TrackViewModel> track);
        Task EnqueueAsync(List<TrackViewModel> tracks, TrackViewModel trackPair);
        Task EnqueueAsync(List<TrackViewModel> tracks);
        Task EnqueueAsync(bool shuffle, bool unshuffle);
        Task EnqueueAsync(List<TrackViewModel> tracks, bool shuffle, bool unshuffle);
        Task EnqueueArtistsAsync(IList<string> artists, bool shuffle, bool unshuffle);
        Task EnqueueGenresAsync(IList<string> genres, bool shuffle, bool unshuffle);
        Task EnqueueAlbumsAsync(IList<string> albumKeys, bool shuffle, bool unshuffle);
        Task StopIfPlayingAsync(TrackViewModel track);
        Task<EnqueueResult> AddToQueueAsync(IList<TrackViewModel> tracks);
        Task<EnqueueResult> AddArtistsToQueueAsync(IList<string> artists);
        Task<EnqueueResult> AddGenresToQueueAsync(IList<string> genres);
        Task<EnqueueResult> AddAlbumsToQueueAsync(IList<string> albumKeys);
        Task<EnqueueResult> AddToQueueNextAsync(IList<TrackViewModel> tracks);
        Task<DequeueResult> DequeueAsync(IList<TrackViewModel> tracks);
        Task<DequeueResult> DequeueAsync(IList<KeyValuePair<string, TrackViewModel>> tracks);
        Task RefreshQueueLanguageAsync();
        Task SaveQueuedTracksAsync();
        Task SavePlaybackCountersAsync();
        void ApplyPreset(EqualizerPreset preset);
        Task SetIsEqualizerEnabledAsync(bool isEnabled);
        Task UpdateQueueMetadataAsync(List<IFileMetadata> fileMetadatas);
        Task UpdateQueueOrderAsync(List<KeyValuePair<string, TrackViewModel>> tracks);
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
        event PlaybackVolumeChangedEventhandler PlaybackVolumeChanged;
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
