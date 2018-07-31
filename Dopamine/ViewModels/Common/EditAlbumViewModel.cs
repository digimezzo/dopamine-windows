﻿using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Utils;
using Dopamine.Services.Cache;
using Dopamine.Services.Dialog;
using Dopamine.Services.Metadata;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using Dopamine.Services.Entities;

namespace Dopamine.ViewModels.Common
{
    public class EditAlbumViewModel : EditMetadataBase
    {
        private AlbumViewModel albumViewModel;
        private IMetadataService metadataService;
        private IDialogService dialogService;
        private ICacheService cacheService;
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
            IDialogService dialogService, ICacheService cacheService) : base(cacheService)
        {
            this.albumViewModel = albumViewModel;
            this.metadataService = metadataService;
            this.dialogService = dialogService;
            this.cacheService = cacheService;

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
                // TODO await this.DownloadArtworkAsync(this.album.AlbumTitle, this.album.AlbumArtist);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not download artwork. Exception: {0}", ex.Message);
            }
        }

        private bool CanDownloadArtwork()
        {
            // TODO if (this.album == null)
            //{
            //    return false;
            //}

            //return !string.IsNullOrEmpty(this.album.AlbumArtist) && !string.IsNullOrEmpty(this.Album.AlbumTitle);
            return false; // TODO
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
                    // TODO await this.metadataService.UpdateAlbumAsync(this.Album, this.Artwork, this.UpdateFileArtwork);
                }
            }
            catch (Exception ex)
            {
                // TODO LogClient.Error("An error occurred while saving the album with title='{0}' and artist='{1}'. Exception: {2}", (string)this.Album.AlbumTitle, (string)this.Album.AlbumArtist, ex.Message);
            }

            this.IsBusy = false;
            return true;
        }
    }
}