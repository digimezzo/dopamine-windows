using System;

namespace Dopamine.Services.Contracts.Playback
{
    public class PlaybackSuccessEventArgs : EventArgs
    {
        public bool IsPlayingPreviousTrack;
        public bool IsSilent;
    }
}