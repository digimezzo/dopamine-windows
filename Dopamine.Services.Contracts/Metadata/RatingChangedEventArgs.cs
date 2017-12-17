using System;

namespace Dopamine.Services.Contracts.Metadata
{
    public class RatingChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public int Rating { get; set; }
    }
}
