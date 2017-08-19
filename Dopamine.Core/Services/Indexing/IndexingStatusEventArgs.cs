using System;

namespace Dopamine.Core.Services.Indexing
{
    public class IndexingStatusEventArgs : EventArgs
    {
        #region Properties
        public IndexingAction IndexingAction { get; set; }
        public long ProgressCurrent { get; set; }
        public long ProgressTotal { get; set; }
        public int ProgressPercent { get; set; }
        #endregion
    }
}
