namespace Dopamine.Common.Services.Playback
{
    public class UpdateQueueMetadataResult
    {
        public bool IsPlayingTrackPlaybackInfoChanged { get; set; }
        public bool IsPlayingTrackArtworkChanged { get; set; }
        public bool IsQueueChanged { get; set; }
    }
}