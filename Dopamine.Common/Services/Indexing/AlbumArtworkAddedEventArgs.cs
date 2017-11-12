using Dopamine.Common.Database.Entities;
using System;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Indexing
{
    public class AlbumArtworkAddedEventArgs : EventArgs
    {
        public List<long> AlbumIds { get; set; }
    }
}