using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.ControlsModule.Views;
using Dopamine.Core.Prism;
using Dopamine.FullPlayerModule.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenViewModel : BindableBase, INavigationAware
    {
        #region Variables
        private bool isShowcaseButtonChecked;
        private IRegionManager regionManager;
        private SlideDirection slideDirection;
        private IPlaybackService playbackService;
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand<bool?> FullPlayerShowcaseButtonCommand { get; set; }
        #endregion

        #region Properties
        public bool IsShowcaseButtonChecked
        {
            get { return this.isShowcaseButtonChecked; }
            set { SetProperty<bool>(ref this.isShowcaseButtonChecked, value); }
        }

        public SlideDirection SlideDirection
        {
            get { return this.slideDirection; }
            set { SetProperty<SlideDirection>(ref this.slideDirection, value); }
        }
        #endregion

        #region Construction
        public NowPlayingScreenViewModel(IRegionManager regionManager, IPlaybackService playbackService)
        {
            this.regionManager = regionManager;
            this.playbackService = playbackService;

            this.playbackService.PlaybackSuccess += (x) => this.SetNowPlaying();

            this.FullPlayerShowcaseButtonCommand = new DelegateCommand<bool?>(iIsShowcaseButtonChecked =>
            {
                this.IsShowcaseButtonChecked = iIsShowcaseButtonChecked.Value;

                this.SetNowPlaying();
            });
            this.SlideDirection = SlideDirection.LeftToRight;
            ApplicationCommands.FullPlayerShowcaseButtonCommand.RegisterCommand(this.FullPlayerShowcaseButtonCommand);
        }
        #endregion

        #region Private
        private void SetNowPlaying()
        {
            if (this.playbackService.Queue.Count > 0)
            {
                if (this.IsShowcaseButtonChecked)
                {
                    this.SlideDirection = SlideDirection.LeftToRight;
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenShowcase).FullName);
                }
                else
                {
                    this.SlideDirection = SlideDirection.RightToLeft;
                    this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenList).FullName);
                }
            }
            else
            {
                this.SlideDirection = SlideDirection.RightToLeft;
                this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NothingPlayingControl).FullName);
            }
        }
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
     
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.SetNowPlaying();
        }
        #endregion
    }
}
