using System;

namespace Dopamine.Common.Audio
{
    public class PlaybackInterruptedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
