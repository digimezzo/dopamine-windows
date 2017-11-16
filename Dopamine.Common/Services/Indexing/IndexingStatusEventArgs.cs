using System;

namespace Dopamine.Common.Services.Indexing
{
    public class IndexingStatusEventArgs : EventArgs
    {
        public IndexingAction IndexingAction { get; set; }
        public long ProgressCurrent { get; set; }
        public int ProgressPercent { get; set; }
    }
}
