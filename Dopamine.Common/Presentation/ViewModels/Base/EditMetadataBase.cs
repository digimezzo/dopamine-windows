using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Api.Lastfm;
using Dopamine.Common.Base;
using Dopamine.Common.Helpers;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Cache;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public class EditMetadataBase : BindableBase
    {
        private bool isBusy;
        private MetadataArtworkValue artwork;
        private string artworkSize;
        private BitmapImage artworkThumbnail;
        private ICacheService cacheService;

        public DelegateCommand DownloadArtworkCommand { get; set; }

        public string ArtworkSize
        {
            get { return this.artworkSize; }
            set { SetProperty<string>(ref this.artworkSize, value); }
        }

        public bool HasArtwork
        {
            get { return Artwork?.Value != null; }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set { SetProperty<bool>(ref this.isBusy, value); }
        }

        public BitmapImage ArtworkThumbnail
        {
            get { return this.artworkThumbnail; }
            set { SetProperty<BitmapImage>(ref this.artworkThumbnail, value); }
        }

        public MetadataArtworkValue Artwork
        {
            get { return this.artwork; }
            set { SetProperty<MetadataArtworkValue>(ref this.artwork, value); }
        }

        public EditMetadataBase(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.artwork = new MetadataArtworkValue();
        }

        private void VisualizeArtwork(byte[] imageData)
        {
            this.ArtworkThumbnail = ImageUtils.ByteToBitmapImage(imageData, 0, 0, Convert.ToInt32(Constants.CoverLargeSize));

            // Size of the artwork
            if (imageData != null)
            {
                // Use PixelWidth and PixelHeight instead of Width and Height:
                // Width and Height take DPI into account. We don't want that here.
                this.ArtworkSize = this.ArtworkThumbnail.PixelWidth + "x" + this.ArtworkThumbnail.PixelHeight;
            }
            else
            {
                this.ArtworkSize = string.Empty;
            }

            RaisePropertyChanged(nameof(this.HasArtwork));
        }

        protected virtual void UpdateArtwork(byte[] imageData)
        {
            // Artwork data
            this.Artwork.Value = imageData;

            // Set the artwork
            this.VisualizeArtwork(imageData);
        }

        protected async Task DownloadArtworkAsync(string albumTitle, string artist)
        {
            this.IsBusy = true;

            try
            {
                Uri artworkUri = await ArtworkHelper.GetAlbumArtworkFromInternetAsync(artist, albumTitle);
                string temporaryFile = await this.cacheService.DownloadFileToTemporaryCacheAsync(artworkUri);
                this.UpdateArtwork(ImageUtils.Image2ByteArray(temporaryFile));
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while downloading artwork for the album title='{0}' and artist='{1}'. Exception: {2}", albumTitle, artist, ex.Message);
            }

            this.IsBusy = false;
        }
    }
}
