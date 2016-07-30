using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Base;
using Dopamine.Core.Database.Entities;
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
        private Album album;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private MetadataArtworkValue artwork;
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

        public Album Album
        {
            get { return this.album; }
            set { SetProperty<Album>(ref this.album, value); }
        }

        public bool UpdateFileArtwork
        {
            get { return this.updateFileArtwork; }
            set { SetProperty<bool>(ref this.updateFileArtwork, value); }
        }
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand ChangeArtworkCommand { get; set; }
        public DelegateCommand RemoveArtworkCommand { get; set; }
        #endregion

        #region Construction
        public EditAlbumViewModel(Album album, IMetadataService metadataService, IDialogService dialogService)
        {
            this.Album = album;
            this.metadataService = metadataService;
            this.dialogService = dialogService;

            this.artwork = new MetadataArtworkValue();

            this.LoadedCommand = new DelegateCommand(async () => await this.GetAlbumArtworkAsync());
            this.ChangeArtworkCommand = new DelegateCommand( async() =>
            {
                if (!await OpenFileUtils.OpenImageFileAsync(new Action<string, byte[]>(this.UpdateArtwork)))
                {
                    this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Changing_Image"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                }
            });


            this.RemoveArtworkCommand = new DelegateCommand(() => this.UpdateArtwork(string.Empty, null));
        }
        #endregion

        #region Private
        private async Task GetAlbumArtworkAsync()
        {
            await Task.Run(() => {
                string artworkPath = ArtworkUtils.GetArtworkPath(this.Album);

                try
                {
                    if (!string.IsNullOrEmpty(artworkPath))
                    {
                        this.UpdateArtwork(artworkPath, ImageOperations.Image2ByteArray(artworkPath));
                    }
                    else
                    {
                        this.UpdateArtwork(string.Empty, null);
                    }

                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occurred while getting the artwork for album with title='{0}' and artist='{1}'. Exception: {2}", this.Album.AlbumTitle, this.Album.AlbumArtist, ex.Message);
                }
            });
        }

        private void UpdateArtwork(string imagePath, byte[] imageData)
        {
            this.Artwork.SetValue(imagePath, imageData);
            this.ArtworkThumbnail = ImageOperations.PathToBitmapImage(imagePath, Convert.ToInt32(Constants.CoverLargeSize), Convert.ToInt32(Constants.CoverLargeSize));
            OnPropertyChanged(() => this.HasArtwork);
        }
        #endregion

        #region Public
        public async Task<bool> SaveAlbumAsync()
        {
            this.IsBusy = true;

            try
            {
                await this.metadataService.UpdateAlbumAsync(this.Album, this.Artwork, this.UpdateFileArtwork);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occurred while saving the album with title='{0}' and artist='{1}'. Exception: {2}", this.Album.AlbumTitle, this.Album.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
            return true;
        }
        #endregion  
    }
}
