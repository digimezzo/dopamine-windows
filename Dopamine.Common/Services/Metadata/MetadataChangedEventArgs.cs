using System;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Metadata
{
    public class MetadataChangedEventArgs : EventArgs
    {
        #region Properties
        public bool IsArtistChanged { get; set; }
        public bool IsGenreChanged { get; set; }
        public bool IsAlbumChanged { get; set; }
        public bool IsTrackChanged { get; set; }
        public bool IsArtworkChanged { get; set; }
        public List<string> ChangedPaths { get; set; }

        public bool IsPlaybackInfoChanged
        {
            get { return this.IsArtistChanged | this.IsAlbumChanged | this.IsTrackChanged; }
        }

        public bool IsMetadataChanged
        {
            get { return this.IsArtistChanged | this.IsGenreChanged | this.IsAlbumChanged | this.IsTrackChanged | this.IsArtworkChanged; }
        }
        #endregion

        #region Construction
        public MetadataChangedEventArgs()
        {
            this.IsArtistChanged = false;
            this.IsGenreChanged = false;
            this.IsAlbumChanged = false;
            this.IsTrackChanged = false;
            this.IsArtworkChanged = false;
        }
        #endregion
    }

}
