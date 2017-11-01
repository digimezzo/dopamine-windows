using System;

namespace Dopamine.Common.Services.Playback
{
    public class PlaybackSuccessEventArgs : EventArgs
    {
        public bool IsPlayingPreviousTrack;
        public bool IsSilent;
    }
}