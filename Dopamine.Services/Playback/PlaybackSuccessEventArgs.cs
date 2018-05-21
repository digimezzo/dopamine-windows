using System;

namespace Dopamine.Services.Playback
{
    public class PlaybackSuccessEventArgs : EventArgs
    {
        public bool IsPlayingPreviousTrack;
        public bool IsSilent;
    }
}