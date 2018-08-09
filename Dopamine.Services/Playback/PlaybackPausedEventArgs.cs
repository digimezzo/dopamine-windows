using System;

namespace Dopamine.Services.Playback
{
    public class PlaybackPausedEventArgs : EventArgs
    {
        public bool IsSilent { get; set; }
    }
}