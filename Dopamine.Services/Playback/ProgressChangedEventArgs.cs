using System;

namespace Dopamine.Services.Playback
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public TimeSpan CurrentTime { get; set; }

        public TimeSpan TotalTime { get; set; }

        public double Progress { get; set; }
    }
}
