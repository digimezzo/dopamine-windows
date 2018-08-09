namespace Dopamine.Services.Playback
{
    public class UpdateQueueMetadataResult
    {
        public bool IsPlayingTrackChanged { get; set; }

        public bool IsQueueChanged { get; set; }
    }
}