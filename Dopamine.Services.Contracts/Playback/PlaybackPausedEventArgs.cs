using System;

namespace Dopamine.Services.Contracts.Playback
{
    public class PlaybackPausedEventArgs : EventArgs
    {
        public bool IsSilent;
    }
}