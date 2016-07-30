using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using Dopamine.Core.Utils;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionCloudViewModel : BindableBase
    {
        #region Variables
        private IAlbumRepository albumRepository;
        private IPlaybackService playbackService;

        private bool hasCloud;
        private AlbumViewModel albumViewModel1;
        private AlbumViewModel albumViewModel2;
        private AlbumViewModel albumViewModel3;
        private AlbumViewModel albumViewModel4;
        private AlbumViewModel albumViewModel5;
        private AlbumViewModel albumViewModel6;
        private AlbumViewModel albumViewModel7;
        private AlbumViewModel albumViewModel8;
        private AlbumViewModel albumViewModel9;
        private AlbumViewModel albumViewModel10;
        private AlbumViewModel albumViewModel11;
        private AlbumViewModel albumViewModel12;
        private AlbumViewModel albumViewModel13;
        private AlbumViewModel albumViewModel14;
        #endregion

        #region Commands
        public DelegateCommand<object> ClickCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        #endregion

        #region Properties
        public bool HasCloud
        {
            get { return this.hasCloud; }
            set { SetProperty<bool>(ref this.hasCloud, value); }
        }
        public AlbumViewModel AlbumViewModel1
        {
            get { return this.albumViewModel1; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel1, value); }
        }

        public AlbumViewModel AlbumViewModel2
        {
            get { return this.albumViewModel2; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel2, value); }
        }

        public AlbumViewModel AlbumViewModel3
        {
            get { return this.albumViewModel3; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel3, value); }
        }

        public AlbumViewModel AlbumViewModel4
        {
            get { return this.albumViewModel4; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel4, value); }
        }

        public AlbumViewModel AlbumViewModel5
        {
            get { return this.albumViewModel5; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel5, value); }
        }

        public AlbumViewModel AlbumViewModel6
        {
            get { return this.albumViewModel6; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel6, value); }
        }

        public AlbumViewModel AlbumViewModel7
        {
            get { return this.albumViewModel7; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel7, value); }
        }

        public AlbumViewModel AlbumViewModel8
        {
            get { return this.albumViewModel8; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel8, value); }
        }

        public AlbumViewModel AlbumViewModel9
        {
            get { return this.albumViewModel9; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel9, value); }
        }

        public AlbumViewModel AlbumViewModel10
        {
            get { return this.albumViewModel10; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel10, value); }
        }

        public AlbumViewModel AlbumViewModel11
        {
            get { return this.albumViewModel11; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel11, value); }
        }

        public AlbumViewModel AlbumViewModel12
        {
            get { return this.albumViewModel12; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel12, value); }
        }

        public AlbumViewModel AlbumViewModel13
        {
            get { return this.albumViewModel13; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel13, value); }
        }

        public AlbumViewModel AlbumViewModel14
        {
            get { return this.albumViewModel14; }
            set { SetProperty<AlbumViewModel>(ref this.albumViewModel14, value); }
        }
        #endregion

        #region Construction
        public CollectionCloudViewModel(IAlbumRepository albumRepository, IPlaybackService playbackService)
        {
            this.albumRepository = albumRepository;
            this.playbackService = playbackService;

            this.playbackService.TrackStatisticsChanged += async (_, __) => await this.PopulateAlbumHistoryAsync();

            this.ClickCommand = new DelegateCommand<object>((album) =>
            {
                try
                {
                    if (album != null)
                    {
                        this.playbackService.Enqueue((Album)album);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occurred during Album enqueue. Exception: {0}", ex.Message);
                }

            });

            this.LoadedCommand = new DelegateCommand(async () =>
            {
                await Task.Delay(Constants.CommonListLoadDelay);
                await this.PopulateAlbumHistoryAsync();
            });

            // Default is True. This prevents the description text of briefly being displayed, even when there is a cloud available. 
            this.HasCloud = true;
        }
        #endregion

        #region Private
        private void UpdateAlbumViewModel(int number, List<Album> albums, ref AlbumViewModel albumViewModel)
        {
            if (albums.Count < number)
            {
                albumViewModel = null;
            }
            else
            {
                Album alb = albums[number - 1];
                if (albumViewModel == null || !albumViewModel.Album.Equals(alb))
                {
                    albumViewModel = new AlbumViewModel
                    {
                        Album = alb,
                        ArtworkPath = ArtworkUtils.GetArtworkPath(alb)
                    };
                }
            }

            OnPropertyChanged("AlbumViewModel" + number.ToString());

            System.Threading.Thread.Sleep(Constants.CloudLoadDelay);
        }

        private async Task PopulateAlbumHistoryAsync()
        {
            var albums = await this.albumRepository.GetAlbumHistoryAsync(14);

            if (albums.Count == 0)
            {
                this.HasCloud = false;
            }
            else
            {
                this.HasCloud = true;

                await Task.Run(() =>
                {
                    this.UpdateAlbumViewModel(1, albums, ref this.albumViewModel1);
                    this.UpdateAlbumViewModel(2, albums, ref this.albumViewModel2);
                    this.UpdateAlbumViewModel(3, albums, ref this.albumViewModel3);
                    this.UpdateAlbumViewModel(4, albums, ref this.albumViewModel4);
                    this.UpdateAlbumViewModel(5, albums, ref this.albumViewModel5);
                    this.UpdateAlbumViewModel(6, albums, ref this.albumViewModel6);
                    this.UpdateAlbumViewModel(7, albums, ref this.albumViewModel7);
                    this.UpdateAlbumViewModel(8, albums, ref this.albumViewModel8);
                    this.UpdateAlbumViewModel(9, albums, ref this.albumViewModel9);
                    this.UpdateAlbumViewModel(10, albums, ref this.albumViewModel10);
                    this.UpdateAlbumViewModel(11, albums, ref this.albumViewModel11);
                    this.UpdateAlbumViewModel(12, albums, ref this.albumViewModel12);
                    this.UpdateAlbumViewModel(13, albums, ref this.albumViewModel13);
                    this.UpdateAlbumViewModel(14, albums, ref this.albumViewModel14);
                });
             }
        }
        #endregion
    }
}
