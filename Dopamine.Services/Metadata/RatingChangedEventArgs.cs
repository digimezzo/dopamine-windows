using System;

namespace Dopamine.Services.Metadata
{
    public class RatingChangedEventArgs : EventArgs
    {
        public string SafePath { get; set; }
        public int Rating { get; set; }
    }
}
