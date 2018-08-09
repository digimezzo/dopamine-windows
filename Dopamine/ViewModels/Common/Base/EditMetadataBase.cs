using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using Dopamine.Data.Metadata;
using Dopamine.Data.Metadata;
using Dopamine.Utils;
using Dopamine.Services.Cache;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Dopamine.ViewModels.Common.Base
{
    public class EditMetadataBase : BindableBase
    {
        private bool isBusy;
        private MetadataArtworkValue artwork;
        private string artworkSize;
        private BitmapImage artworkThumbnail;
        private ICacheService cacheService;

        public DelegateCommand DownloadArtworkCommand { get; set; }
        public DelegateCommand ExportArtworkCommand { get; set; }

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

            this.ExportArtworkCommand = new DelegateCommand(async () => await this.ExportArtworkAsync(), () => this.CanExportArtwork());
        }

        private async Task ExportArtworkAsync()
        {
            if (this.HasArtwork)
            {
                await SaveFileUtils.SaveImageFileAsync("cover", this.Artwork.Value);
            }
        }

        private bool CanExportArtwork()
        {
            return this.HasArtwork;
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

        protected virtual void ShowArtwork(byte[] imageData)
        {
            this.Artwork = new MetadataArtworkValue(imageData); // Create new artwork data, so IsValueChanged is not triggered.
            this.VisualizeArtwork(imageData); // Visualize the artwork
            this.ExportArtworkCommand.RaiseCanExecuteChanged();
        }

        protected virtual void UpdateArtwork(byte[] imageData)
        {
            this.Artwork.Value = imageData; // Update existing artwork data, so IsValueChanged is triggered.
            this.VisualizeArtwork(imageData); // Visualize the artwork
            this.ExportArtworkCommand.RaiseCanExecuteChanged();
        }

        protected async Task DownloadArtworkAsync(string title, IList<string> artists, string alternateTitle = "", IList<string> alternateArtists = null)
        {
            this.IsBusy = true;

            try
            {
                Uri artworkUri = await ArtworkUtils.GetAlbumArtworkFromInternetAsync(title, artists, alternateTitle, alternateArtists);

                if(artworkUri != null)
                {
                    string temporaryFile = await this.cacheService.DownloadFileToTemporaryCacheAsync(artworkUri);
                    this.UpdateArtwork(ImageUtils.Image2ByteArray(temporaryFile, 0, 0));
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while downloading artwork. Exception: {0}", ex.Message);
            }

            this.IsBusy = false;
        }
    }
}
