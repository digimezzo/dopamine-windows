using System;

namespace Dopamine.Services.Metadata
{
    public class LoveChangedEventArgs : EventArgs
    {
        public string SafePath { get; set; }
        public bool Love { get; set; }
    }
}
