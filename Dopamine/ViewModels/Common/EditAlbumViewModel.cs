using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Services.Cache;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.InfoDownload;
using Dopamine.Services.Metadata;
using Dopamine.Utils;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using System;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common
{
    public class EditAlbumViewModel : EditMetadataBase
    {
        private AlbumViewModel albumViewModel;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private ICacheService cacheService;
        private IInfoDownloadService infoDownloadService;
        private bool updateFileArtwork;

        public AlbumViewModel AlbumViewModel
        {
            get { return this.albumViewModel; }
            set
            {
                SetProperty(ref this.albumViewModel, value);
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

        public EditAlbumViewModel(AlbumViewModel albumViewModel, IMetadataService metadataService,
            IDialogService dialogService, ICacheService cacheService, IInfoDownloadService infoDownloadService) : base(cacheService, infoDownloadService)
        {
            this.albumViewModel = albumViewModel;
            this.metadataService = metadataService;
            this.dialogService = dialogService;
            this.cacheService = cacheService;
            this.infoDownloadService = infoDownloadService;

            this.LoadedCommand = new DelegateCommand(async () => await this.GetAlbumArtworkAsync());

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
                await this.DownloadArtworkAsync(this.albumViewModel.AlbumTitle, this.albumViewModel.AlbumArtists);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not download artwork. Exception: {0}", ex.Message);
            }
        }

        private bool CanDownloadArtwork()
        {
            if (this.albumViewModel == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(this.albumViewModel.AlbumArtist) && !string.IsNullOrEmpty(this.albumViewModel.AlbumTitle);
        }

        protected override void UpdateArtwork(byte[] imageData)
        {
            base.UpdateArtwork(imageData);
        }

        private async Task GetAlbumArtworkAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.albumViewModel.ArtworkPath))
                    {
                        this.ShowArtwork(ImageUtils.Image2ByteArray(this.albumViewModel.ArtworkPath, 0, 0));
                    }
                    else
                    {
                        this.ShowArtwork(null);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("An error occurred while getting the artwork for album title='{0}' and artist='{1}'. Exception: {2}", (string)this.albumViewModel.AlbumTitle, (string)this.albumViewModel.AlbumArtist, ex.Message);
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
                    await this.metadataService.UpdateAlbumAsync(this.albumViewModel, this.Artwork, this.UpdateFileArtwork);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while saving the album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.albumViewModel.AlbumTitle, (string)this.albumViewModel.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
            return true;
        }
    }
}