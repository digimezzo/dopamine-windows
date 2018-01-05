using System;

namespace Dopamine.Services.Contracts.Metadata
{
    public class LoveChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public bool Love { get; set; }
    }
}
