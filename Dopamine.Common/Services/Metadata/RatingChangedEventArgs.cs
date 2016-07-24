using System;

namespace Dopamine.Common.Services.Metadata
{
    public class RatingChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public int Rating { get; set; }
    }
}
