namespace Dopamine.Common.Services.Metadata
{
    public class AlbumMetadataChangeStatus
    {
        #region Properties
        public bool IsAlbumTitleChanged { get; set; }
        public bool IsAlbumArtistChanged { get; set; }
        public bool IsAlbumYearChanged { get; set; }
        public bool IsAlbumArtworkChanged { get; set; }
        #endregion

        #region Construction
        public AlbumMetadataChangeStatus()
        {
            this.IsAlbumTitleChanged = false;
            this.IsAlbumArtistChanged = false;
            this.IsAlbumYearChanged = false;
            this.IsAlbumArtworkChanged = false;
        }
        #endregion
    }
}
