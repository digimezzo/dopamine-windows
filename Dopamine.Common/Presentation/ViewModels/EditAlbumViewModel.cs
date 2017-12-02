using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Metadata;
using Prism.Commands;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class EditAlbumViewModel : EditMetadataBase
    {
        private Common.Database.Entities.Album album;
        private IAlbumRepository albumRepository;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private ICacheService cacheService;
        private bool updateFileArtwork;

        public Album Album
        {
            get { return this.album; }
            set {
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
        public DelegateCommand ExportArtworkCommand { get; set; }
        public DelegateCommand ChangeArtworkCommand { get; set; }
        public DelegateCommand RemoveArtworkCommand { get; set; }
    
        public EditAlbumViewModel(Album album, IMetadataService metadataService, 
            IDialogService dialogService, ICacheService cacheService, IAlbumRepository albumRepository) : base(cacheService)
        {
            this.albumRepository = albumRepository;
            this.metadataService = metadataService;
            this.dialogService = dialogService;
            this.cacheService = cacheService;

            this.LoadedCommand = new DelegateCommand(async() => await this.GetAlbumArtworkAsync(album));

            this.ExportArtworkCommand = new DelegateCommand(async () =>
            {
                if (HasArtwork)
                {
                    await SaveFileUtils.SaveImageFileAsync("cover", this.Artwork.Value);
                }
            });

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
            await this.DownloadArtworkAsync(this.album.AlbumTitle, this.album.AlbumArtist);
        }

        private bool CanDownloadArtwork()
        {
            if(this.album == null)
            {
                return false;
            }

            return this.album.AlbumArtist != Defaults.UnknownArtistText && this.Album.AlbumTitle != Defaults.UnknownAlbumText;
        }
      
        private async Task GetAlbumArtworkAsync(Album album)
        {
            await Task.Run(() =>
            {
                this.Album = this.albumRepository.GetAlbum(album.AlbumID);
                string artworkPath = this.cacheService.GetCachedArtworkPath((string)this.Album.ArtworkID);

                try
                {
                    if (!string.IsNullOrEmpty(artworkPath))
                    {
                        this.UpdateArtwork(ImageUtils.Image2ByteArray(artworkPath));
                    }
                    else
                    {
                        this.UpdateArtwork(null);
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
                await this.metadataService.UpdateAlbumAsync((Common.Database.Entities.Album)this.Album, this.Artwork, this.UpdateFileArtwork);
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