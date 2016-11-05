using System;

namespace Dopamine.Common.Services.Metadata
{
    public class LyricsChangedEventArgs : EventArgs
    {
        public string Lyrics { get; set; }
    }
}
