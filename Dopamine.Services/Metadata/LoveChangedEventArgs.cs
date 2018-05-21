using System;

namespace Dopamine.Services.Metadata
{
    public class LoveChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public bool Love { get; set; }
    }
}
