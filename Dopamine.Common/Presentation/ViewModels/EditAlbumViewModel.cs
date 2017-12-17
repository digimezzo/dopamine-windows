using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Core.Base;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories.Interfaces;
using Dopamine.Presentation.Utils;
using Dopamine.Services.Cache;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Dialog;
using Prism.Commands;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class EditAlbumViewModel : EditMetadataBase
    {
        private Data.Entities.Album album;
        private IAlbumRepository albumRepository;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private ICacheService cacheService;
        private bool updateFileArtwork;

        public Album Album
        {
            get { return this.album; }
            set
            {
                SetProperty(ref this.album, value);
                this.DownloadArtworkCommand.RaiseCanExecuteChanged();
            }
        }

        public bool UpdateFileArtwork
        {
            get { return this.updateFileArtwork; }
            set { SetProperty<bool>(ref this.updateFileArtwork, value); }
        }

        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand ChangeArtworkCommand { get; set; }
        public DelegateCommand RemoveArtworkCommand { get; set; }

        public EditAlbumViewModel(long albumId, IMetadataService metadataService,
            IDialogService dialogService, ICacheService cacheService, IAlbumRepository albumRepository) : base(cacheService)
        {
            this.albumRepository = albumRepository;
            this.metadataService = metadataService;
            this.dialogService = dialogService;
            this.cacheService = cacheService;

            this.LoadedCommand = new DelegateCommand(async () => await this.GetAlbumArtworkAsync(albumId));

            this.ChangeArtworkCommand = new DelegateCommand(async () =>
           {
               if (!await OpenFileUtils.OpenImageFileAsync(new Action<byte[]>(this.UpdateArtwork)))
               {
                   this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Changing_Image"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
               }
           });


            this.RemoveArtworkCommand = new DelegateCommand(() => this.UpdateArtwork(null));
            this.DownloadArtworkCommand = new DelegateCommand(async () => await this.DownloadArtworkAsync(), () => this.CanDownloadArtwork());
        }

        private async Task DownloadArtworkAsync()
        {
            try
            {
                await this.DownloadArtworkAsync(this.album.AlbumTitle, this.album.AlbumArtist);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not download artwork. Exception: {0}", ex.Message);
            }
        }

        private bool CanDownloadArtwork()
        {
            if (this.album == null)
            {
                return false;
            }

            return this.album.AlbumArtist != Defaults.UnknownArtistText && this.Album.AlbumTitle != Defaults.UnknownAlbumText;
        }

        protected override void UpdateArtwork(byte[] imageData)
        {
            base.UpdateArtwork(imageData);
        }

        private async Task GetAlbumArtworkAsync(long albumId)
        {
            await Task.Run(() =>
            {
                this.Album = this.albumRepository.GetAlbum(albumId);
                string artworkPath = this.cacheService.GetCachedArtworkPath((string)this.Album.ArtworkID);

                try
                {
                    if (!string.IsNullOrEmpty(artworkPath))
                    {
                        this.ShowArtwork(ImageUtils.Image2ByteArray(artworkPath));
                    }
                    else
                    {
                        this.ShowArtwork(null);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("An error occurred while getting the artwork for album title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
                }
            });
        }

        public async Task<bool> SaveAlbumAsync()
        {
            this.IsBusy = true;

            try
            {
                if (this.Artwork.IsValueChanged)
                {
                    await this.metadataService.UpdateAlbumAsync(this.Album, this.Artwork, this.UpdateFileArtwork);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while saving the album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
            return true;
        }
    }
}