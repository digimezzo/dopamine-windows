using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Api.Lastfm;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class EditAlbumViewModel : BindableBase
    {
        #region Variables
        private bool isBusy;
        private Core.Database.Entities.Album album;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private ICacheService cacheService;
        private MetadataArtworkValue artwork;
        private string artworkSize;
        private BitmapImage artworkThumbnail;
        private bool updateFileArtwork;
        #endregion

        #region ReadOnly Properties
        public bool HasArtwork
        {
            get { return this.Artwork != null && this.Artwork != null; }
        }
        #endregion

        #region Properties
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

        public Core.Database.Entities.Album Album
        {
            get { return this.album; }
            set { base.SetProperty(ref this.album, value); }
        }

        public bool UpdateFileArtwork
        {
            get { return this.updateFileArtwork; }
            set { SetProperty<bool>(ref this.updateFileArtwork, value); }
        }

        public string ArtworkSize
        {
            get { return this.artworkSize; }
            set { SetProperty<string>(ref this.artworkSize, value); }
        }
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand ChangeArtworkCommand { get; set; }
        public DelegateCommand RemoveArtworkCommand { get; set; }
        public DelegateCommand DownloadArtworkCommand { get; set; }
        #endregion

        #region Construction
        public EditAlbumViewModel(Core.Database.Entities.Album album, IMetadataService metadataService, IDialogService dialogService, ICacheService cacheService)
        {
            this.Album = album;
            this.metadataService = metadataService;
            this.dialogService = dialogService;
            this.cacheService = cacheService;

            this.artwork = new MetadataArtworkValue();

            this.LoadedCommand = new DelegateCommand(async () => await this.GetAlbumArtworkAsync());
            this.ChangeArtworkCommand = new DelegateCommand(async () =>
           {
               if (!await OpenFileUtils.OpenImageFileAsync(new Action<byte[]>(this.UpdateArtwork)))
               {
                   this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Changing_Image"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
               }
           });


            this.RemoveArtworkCommand = new DelegateCommand(() => this.UpdateArtwork(null));
            this.DownloadArtworkCommand = new DelegateCommand(() => this.DownloadArtworkAsync(), () => this.album.AlbumArtist != Defaults.UnknownAlbumArtistString && this.Album.AlbumTitle != Defaults.UnknownAlbumString);
        }
        #endregion

        #region Private
        private async Task GetAlbumArtworkAsync()
        {
            await Task.Run(() =>
            {
                string artworkPath = this.cacheService.GetCachedArtworkPath((string)this.Album.ArtworkID);

                try
                {
                    if (!string.IsNullOrEmpty(artworkPath))
                    {
                        this.UpdateArtwork(ImageOperations.Image2ByteArray(artworkPath));
                    }
                    else
                    {
                        this.UpdateArtwork(null);
                    }

                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occurred while getting the artwork for album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
                }
            });
        }

        private void VisualizeArtwork(byte[] imageData)
        {
            this.ArtworkThumbnail = ImageOperations.ByteToBitmapImage(imageData, 0, 0, Convert.ToInt32(Constants.CoverLargeSize));

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

            OnPropertyChanged(() => this.HasArtwork);
        }

        private void UpdateArtwork(byte[] imageData)
        {
            // Artwork data
            this.Artwork.Value = imageData;

            // Set the artwork
            this.VisualizeArtwork(imageData);
        }

        private async Task DownloadArtworkAsync()
        {
            this.IsBusy = true;

            try
            {
                Core.Api.Lastfm.Album lfmAlbum = await LastfmApi.AlbumGetInfo((string)this.Album.AlbumArtist, (string)this.Album.AlbumTitle, false, "EN");
                byte[] artworkData = null;

                if (!string.IsNullOrEmpty(lfmAlbum.LargestImage()))
                {
                    string temporaryFilePath = await this.cacheService.DownloadFileToTemporaryCacheAsync(new Uri(lfmAlbum.LargestImage()));

                    if (!string.IsNullOrEmpty(temporaryFilePath))
                    {
                        this.UpdateArtwork(ImageOperations.Image2ByteArray(temporaryFilePath));
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occurred while downloading artwork for the album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
        }
        #endregion

        #region Public
        public async Task<bool> SaveAlbumAsync()
        {
            this.IsBusy = true;

            try
            {
                await this.metadataService.UpdateAlbumAsync((Core.Database.Entities.Album)this.Album, this.Artwork, this.UpdateFileArtwork);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occurred while saving the album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
            return true;
        }
        #endregion
    }
}
