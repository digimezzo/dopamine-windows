namespace Dopamine.Common.Services.Playback
{
    public class PlaybackCounter
    {
        #region Properties
        public string Path { get; set; }
        public int PlayCount { get; set; }
        public int SkipCount { get; set; }
        public long DateLastPlayed { get; set; }
        #endregion
    }
}
