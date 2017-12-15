using System;

namespace Dopamine.Services.Metadata
{
    public class MetadataChangedEventArgs : EventArgs
    {
        public bool IsArtistChanged { get; set; }
        public bool IsGenreChanged { get; set; }
        public bool IsAlbumChanged { get; set; }
        public bool IsTrackChanged { get; set; }
        public bool IsArtworkChanged { get; set; }

        public bool IsMetadataChanged
        {
            get { return this.IsArtistChanged | this.IsGenreChanged | this.IsAlbumChanged | this.IsTrackChanged | this.IsArtworkChanged; }
        }
     
        public MetadataChangedEventArgs()
        {
            this.IsArtistChanged = false;
            this.IsGenreChanged = false;
            this.IsAlbumChanged = false;
            this.IsTrackChanged = false;
            this.IsArtworkChanged = false;
        }
    }

}
