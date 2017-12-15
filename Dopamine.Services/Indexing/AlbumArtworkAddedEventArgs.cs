using System;
using System.Collections.Generic;

namespace Dopamine.Services.Indexing
{
    public class AlbumArtworkAddedEventArgs : EventArgs
    {
        public List<long> AlbumIds { get; set; }
    }
}