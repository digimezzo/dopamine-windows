using System;

namespace Dopamine.Common.Services.Playback
{
    public class PlaybackPausedEventArgs : EventArgs
    {
        public bool IsSilent;
    }
}