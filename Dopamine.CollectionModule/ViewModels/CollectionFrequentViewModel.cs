using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Indexing;
using Dopamine.Common.Services.Playback;
using Dopamine.ControlsModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionFrequentViewModel : BindableBase, INavigationAware
    {
        #region Variables
        private IAlbumRepository albumRepository;
        private IPlaybackService playbackService;
        private IIndexingService indexingService;
        private ICacheService cacheService;
        private IRegionManager regionManager;

        private bool isFirstLoad = true;

        private AlbumViewModel albumViewModel1;
        private AlbumViewModel albumViewModel2;
        private AlbumViewModel albumViewModel3;
        private AlbumViewModel albumViewModel4;
        private AlbumViewModel albumViewModel5;
        private AlbumViewModel albumViewModel6;
        #endregion

        #region Commands
        public DelegateCommand<object> ClickCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        #endregion

        #region Properties
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
        #endregion

        #region Construction
        public CollectionFrequentViewModel(IAlbumRepository albumRepository, IPlaybackService playbackService, ICacheService cacheService, IIndexingService indexingService, IRegionManager regionManager)
        {
            this.albumRepository = albumRepository;
            this.playbackService = playbackService;
            this.cacheService = cacheService;
            this.indexingService = indexingService;
            this.regionManager = regionManager;

            this.playbackService.PlaybackCountersChanged += async (_, __) => await this.PopulateAlbumHistoryAsync();
            this.indexingService.IndexingStopped += async (_, __) => await this.PopulateAlbumHistoryAsync();

            this.ClickCommand = new DelegateCommand<object>((album) =>
            {
                try
                {
                    if (album != null)
                    {
                        this.playbackService.EnqueueAsync((Album)album);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("An error occurred during Album enqueue. Exception: {0}", ex.Message);
                }

            });

            this.LoadedCommand = new DelegateCommand(async () =>
            {
                if (!isFirstLoad) return;

                isFirstLoad = false;

                await Task.Delay(Constants.CommonListLoadDelay);
                await this.PopulateAlbumHistoryAsync();
            });
        }
        #endregion

        #region Private
        private void UpdateAlbumViewModel(int number, List<Album> albums, ref AlbumViewModel albumViewModel)
        {
            if (albums.Count >= number)
            {
                Album alb = albums[number - 1];

                if (albumViewModel == null || !albumViewModel.Album.Equals(alb))
                {
                    albumViewModel = new AlbumViewModel
                    {
                        Album = alb,
                        ArtworkPath = this.cacheService.GetCachedArtworkPath(alb.ArtworkID)
                    };
                }
            }
            else
            {
                // Shows an empty tile
                albumViewModel = new AlbumViewModel
                {
                    Album = new Album() { AlbumTitle = string.Empty, AlbumArtist = string.Empty },
                    ArtworkPath = string.Empty,
                    Opacity = 0.8 - (number / 10.0)
                };
            }

            OnPropertyChanged("AlbumViewModel" + number.ToString());
            System.Threading.Thread.Sleep(Constants.CloudLoadDelay);
        }

        private async Task PopulateAlbumHistoryAsync()
        {
            var albums = await this.albumRepository.GetFrequentAlbumsAsync(6);

            await Task.Run(() =>
            {
                this.UpdateAlbumViewModel(1, albums, ref this.albumViewModel1);
                this.UpdateAlbumViewModel(2, albums, ref this.albumViewModel2);
                this.UpdateAlbumViewModel(3, albums, ref this.albumViewModel3);
                this.UpdateAlbumViewModel(4, albums, ref this.albumViewModel4);
                this.UpdateAlbumViewModel(5, albums, ref this.albumViewModel5);
                this.UpdateAlbumViewModel(6, albums, ref this.albumViewModel6);
            });
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.regionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, typeof(Empty).FullName);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.regionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, typeof(SearchControl).FullName);
        }
        #endregion
    }
}
