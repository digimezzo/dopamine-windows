using System;

namespace Dopamine.Services.Metadata
{
    public class MetadataChangedEventArgs : EventArgs
    {
        public bool IsOnlyArtworkChanged { get; }

        public MetadataChangedEventArgs(bool isOnlyArtworkChanged = false)
        {
            this.IsOnlyArtworkChanged = IsOnlyArtworkChanged;
        }
    }

}
