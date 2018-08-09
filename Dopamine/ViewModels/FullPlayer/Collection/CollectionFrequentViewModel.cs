using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Indexing;
using Dopamine.Services.Playback;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionFrequentViewModel : BindableBase
    {
        private IPlaybackService playbackService;
        private IIndexingService indexingService;
        private ICacheService cacheService;
        private IRegionManager regionManager;
        private ITrackRepository trackRepository;
        private IAlbumArtworkRepository albumArtworkRepository;
        private AlbumViewModel albumViewModel1;
        private AlbumViewModel albumViewModel2;
        private AlbumViewModel albumViewModel3;
        private AlbumViewModel albumViewModel4;
        private AlbumViewModel albumViewModel5;
        private AlbumViewModel albumViewModel6;
        private bool isFirstLoad = true;

        public DelegateCommand<object> ClickCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }

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

        public CollectionFrequentViewModel(IContainerProvider container)
        {
            // Dependency injection
            this.playbackService = container.Resolve<IPlaybackService>();
            this.cacheService = container.Resolve<ICacheService>();
            this.indexingService = container.Resolve<IIndexingService>();
            this.regionManager = container.Resolve<IRegionManager>();
            this.trackRepository = container.Resolve<ITrackRepository>();
            this.albumArtworkRepository = container.Resolve<IAlbumArtworkRepository>();

            // Events
            this.playbackService.PlaybackCountersChanged += async (_) => await this.PopulateAlbumHistoryAsync();
            this.indexingService.IndexingStopped += async (_, __) => await this.PopulateAlbumHistoryAsync();

            // Commands
            this.ClickCommand = new DelegateCommand<object>((albumViewModel) =>
            {
                try
                {
                    if (albumViewModel != null)
                    {
                        this.playbackService.EnqueueAlbumsAsync(new List<AlbumViewModel> { (AlbumViewModel)albumViewModel }, false, false);
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

        private void UpdateAlbumViewModel(int number, IList<AlbumData> albumDatas, ref AlbumViewModel albumViewModel)
        {
            if (albumDatas.Count >= number)
            {
                AlbumData data = albumDatas[number - 1];

                if (albumViewModel == null || !albumViewModel.AlbumKey.Equals(data.AlbumKey))
                {
                    Task<AlbumArtwork> task = this.albumArtworkRepository.GetAlbumArtworkAsync(data.AlbumKey);
                    AlbumArtwork albumArtwork = task.Result;

                    albumViewModel = new AlbumViewModel(data, true)
                    {
                        ArtworkPath = this.cacheService.GetCachedArtworkPath(albumArtwork.ArtworkID)
                    };
                }
            }
            else
            {
                // Shows an empty tile
                albumViewModel = new AlbumViewModel(AlbumData.CreateDefault(), false)
                {
                    ArtworkPath = string.Empty,
                    Opacity = 0.8 - (number / 10.0)
                };
            }

            RaisePropertyChanged("AlbumViewModel" + number.ToString());
            System.Threading.Thread.Sleep(Constants.CloudLoadDelay);
        }

        private async Task PopulateAlbumHistoryAsync()
        {
            IList<AlbumData> albumDatas = await this.trackRepository.GetFrequentAlbumDataAsync(6);

            await Task.Run(() =>
            {
                this.UpdateAlbumViewModel(1, albumDatas, ref this.albumViewModel1);
                this.UpdateAlbumViewModel(2, albumDatas, ref this.albumViewModel2);
                this.UpdateAlbumViewModel(3, albumDatas, ref this.albumViewModel3);
                this.UpdateAlbumViewModel(4, albumDatas, ref this.albumViewModel4);
                this.UpdateAlbumViewModel(5, albumDatas, ref this.albumViewModel5);
                this.UpdateAlbumViewModel(6, albumDatas, ref this.albumViewModel6);
            });
        }
    }
}
