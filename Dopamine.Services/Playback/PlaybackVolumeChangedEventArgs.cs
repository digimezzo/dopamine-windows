namespace Dopamine.Services.Playback
{
    public class PlaybackVolumeChangedEventArgs
    {
        public bool IsChangedWhileLoadingSettings { get; private set; }

        public PlaybackVolumeChangedEventArgs(bool isChangedWhileLoadingSettings)
        {
            this.IsChangedWhileLoadingSettings = isChangedWhileLoadingSettings;
        }
    }
}
