using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Data;
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
    public delegate void PlaybackCountersChangedEventHandler(IList<PlaybackCounter> counters);
    public delegate void PlaybackVolumeChangedEventhandler(object sender, PlaybackVolumeChangedEventArgs e);

    public interface IPlaybackService
    {
        IPlayer Player { get; }

        TrackViewModel CurrentTrack { get; }

        bool HasQueue { get; }

        bool HasCurrentTrack { get; }

        bool IsSavingQueuedTracks { get; }

        bool IsSavingPlaybackCounters { get; }

        bool HasMediaFoundationSupport { get; }

        IList<TrackViewModel> Queue { get; }

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

        void Stop();

        void SkipProgress(double progress);

        void SkipSeconds(int jumpSeconds);

        void SetMute(bool mute);

        Task SetShuffleAsync(bool shuffle);

        Task PlayNextAsync();

        Task PlayPreviousAsync();

        Task PlayOrPauseAsync();

        Task PlaySelectedAsync(TrackViewModel track);

        Task<bool> PlaySelectedAsync(IList<TrackViewModel> tracks);

        Task EnqueueAsync(IList<TrackViewModel> tracks, TrackViewModel track);

        Task EnqueueAsync(IList<TrackViewModel> tracks);

        Task EnqueueAsync(bool shuffle, bool unshuffle);

        Task EnqueueAsync(IList<TrackViewModel> tracks, bool shuffle, bool unshuffle);

        Task EnqueueArtistsAsync(IList<string> artists, bool shuffle, bool unshuffle);

        Task EnqueueGenresAsync(IList<string> genres, bool shuffle, bool unshuffle);

        Task EnqueueAlbumsAsync(IList<AlbumViewModel> albumViewModels, bool shuffle, bool unshuffle);

        Task EnqueuePlaylistsAsync(IList<PlaylistViewModel> playlistViewModels, bool shuffle, bool unshuffle);

        Task StopIfPlayingAsync(TrackViewModel track);

        Task<EnqueueResult> AddToQueueAsync(IList<TrackViewModel> tracks);

        Task<EnqueueResult> AddArtistsToQueueAsync(IList<string> artists);

        Task<EnqueueResult> AddGenresToQueueAsync(IList<string> genres);

        Task<EnqueueResult> AddAlbumsToQueueAsync(IList<AlbumViewModel> albumViewModels);

        Task<EnqueueResult> AddToQueueNextAsync(IList<TrackViewModel> tracks);

        Task<DequeueResult> DequeueAsync(IList<TrackViewModel> tracks);

        Task SaveQueuedTracksAsync();

        Task SavePlaybackCountersAsync();

        void ApplyPreset(EqualizerPreset preset);

        Task SetIsEqualizerEnabledAsync(bool isEnabled);

        Task UpdateQueueMetadataAsync(IList<FileMetadata> fileMetadatas);

        Task UpdateQueueOrderAsync(IList<TrackViewModel> tracks);

        Task<IList<AudioDevice>> GetAllAudioDevicesAsync();

        Task SwitchAudioDeviceAsync(AudioDevice audioDevice);

        Task<AudioDevice> GetSavedAudioDeviceAsync();

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
        event Action<int> AddedTracksToQueue;
        event PlaybackCountersChangedEventHandler PlaybackCountersChanged;
        event Action<bool> LoadingTrack;
        event EventHandler PlayingTrackChanged;
        event EventHandler QueueChanged;
    }
}
