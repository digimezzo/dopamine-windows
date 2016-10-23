using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playback
{
    public delegate void PlaybackFailedEventHandler(object sender, PlaybackFailedEventArgs e);

    public interface IPlaybackService
    {
        #region ReadOnly Properties
        IPlayer Player { get; }
        TrackInfo PlayingTrack { get; }
        bool IsSavingQueuedTracks { get; }
        bool IsSavingTrackStatistics { get; }
        bool NeedsSavingQueuedTracks { get; }
        bool NeedsSavingTrackStatistics { get; }
        List<TrackInfo> Queue { get; }
        bool Shuffle { get; }
        bool Mute { get; }
        bool IsStopped { get; }
        bool IsPlaying { get; }
        TimeSpan GetCurrentTime { get; }
        TimeSpan GetTotalTime { get; }
        #endregion

        #region Properties
        double Progress { get; set; }
        float Volume { get; set; }
        LoopMode LoopMode { get; set; }
        int Latency { get; set; }
        bool EventMode { get; set; }
        bool ExclusiveMode { get; set; }
        bool IsSpectrumVisible { get; set; }
        #endregion

        #region Functions
        Task SuspendAsync();
        Task UnsuspendAsync();
        void Stop();
        void Skip(double progress);
        void SetMute(bool mute);
        Task SetShuffle(bool shuffle);
        Task PlayNextAsync();
        Task PlayPreviousAsync();
        Task PlayOrPauseAsync();
        Task PlaySelectedAsync(TrackInfo selectedTrack);
        Task Enqueue();
        Task Enqueue(List<TrackInfo> tracks, TrackInfo selectedTrack);
        Task Enqueue(List<TrackInfo> tracks);
        Task Enqueue(Artist artist);
        Task Enqueue(Genre genre);
        Task Enqueue(Album album);
        Task Enqueue(Playlist playlist);
        Task<AddToQueueResult> AddToQueue(IList<TrackInfo> tracks);
        Task<AddToQueueResult> AddToQueue(IList<Artist> artists);
        Task<AddToQueueResult> AddToQueue(IList<Genre> genres);
        Task<AddToQueueResult> AddToQueue(IList<Album> albums);
        Task<AddToQueueResult> AddToQueue(IList<Playlist> playlists);
        Task<DequeueResult> Dequeue(IList<TrackInfo> selectedTracks);
        Task SaveQueuedTracksAsync();
        Task SaveTrackStatisticsAsync();
        void ApplyPreset(EqualizerPreset preset);
        void SetIsEqualizerEnabled(bool isEnabled);
        #endregion

        #region Events
        event Action<bool> PlaybackSuccess;
        event PlaybackFailedEventHandler PlaybackFailed;
        event EventHandler PlaybackStopped;
        event EventHandler PlaybackPaused;
        event EventHandler PlaybackResumed;
        event EventHandler PlaybackProgressChanged;
        event EventHandler PlaybackVolumeChanged;
        event EventHandler PlaybackMuteChanged;
        event EventHandler PlaybackLoopChanged;
        event EventHandler PlaybackShuffleChanged;
        event Action<bool> SpectrumVisibilityChanged;
        event EventHandler ShuffledTracksChanged;
        event Action<int> AddedTracksToQueue;
        event EventHandler TrackStatisticsChanged;
        event Action<bool> LoadingTrack;
        #endregion
    }
}
