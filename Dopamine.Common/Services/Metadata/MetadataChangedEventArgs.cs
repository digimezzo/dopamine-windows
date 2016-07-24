using System;

namespace Dopamine.Common.Services.Metadata
{
    public class MetadataChangedEventArgs : EventArgs
    {
        #region Properties
        public bool IsArtistMetadataChanged { get; set; }
        public bool IsGenreMetadataChanged { get; set; }
        public bool IsAlbumTitleMetadataChanged { get; set; }
        public bool IsAlbumArtistMetadataChanged { get; set; }
        public bool IsAlbumYearMetadataChanged { get; set; }
        public bool IsAlbumArtworkMetadataChanged { get; set; }
        public bool IsTrackMetadataChanged { get; set; }

        public bool IsMetadataChanged
        {
            get { return this.IsArtistMetadataChanged | this.IsGenreMetadataChanged | this.IsAlbumTitleMetadataChanged | this.IsAlbumArtistMetadataChanged | this.IsAlbumYearMetadataChanged | this.IsAlbumArtworkMetadataChanged | this.IsTrackMetadataChanged; }
        }
        #endregion

        #region Construction
        public MetadataChangedEventArgs()
        {
            this.IsArtistMetadataChanged = false;
            this.IsGenreMetadataChanged = false;
            this.IsAlbumTitleMetadataChanged = false;
            this.IsAlbumArtistMetadataChanged = false;
            this.IsAlbumYearMetadataChanged = false;
            this.IsAlbumArtworkMetadataChanged = false;
            this.IsTrackMetadataChanged = false;
        }
        #endregion
    }

}
