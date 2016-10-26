using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.ControlsModule.Views;
using Dopamine.Core.Prism;
using Dopamine.FullPlayerModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenViewModel : BindableBase, INavigationAware
    {
        #region Variables
        private bool isShowcaseButtonChecked;
        private IRegionManager regionManager;
        private IPlaybackService playbackService;
        private SlideDirection slideDirection;
        private bool isShowCaseVisible;
        private bool isPlaylistVisible;
        private bool isLyricsVisible;
        private bool isArtistInformationVisible;
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand NowPlayingScreenPlaylistButtonCommand { get; set; }
        public DelegateCommand NowPlayingScreenShowcaseButtonCommand { get; set; }
        public DelegateCommand NowPlayingScreenLyricsButtonCommand { get; set; }
        public DelegateCommand NowPlayingScreenArtistInformationButtonCommand { get; set; }
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

            this.isPlaylistVisible = true; // default

            this.playbackService.PlaybackSuccess += (_) => this.SetNowPlaying();

            this.NowPlayingScreenShowcaseButtonCommand = new DelegateCommand(() => this.SetShowCase());
            this.NowPlayingScreenPlaylistButtonCommand = new DelegateCommand(() => this.SetPlaylist());
            this.NowPlayingScreenLyricsButtonCommand = new DelegateCommand(() => this.SetLyrics());
            this.NowPlayingScreenArtistInformationButtonCommand = new DelegateCommand(() => this.SetArtistInformation());

            ApplicationCommands.NowPlayingScreenShowcaseButtonCommand.RegisterCommand(this.NowPlayingScreenShowcaseButtonCommand);
            ApplicationCommands.NowPlayingScreenPlaylistButtonCommand.RegisterCommand(this.NowPlayingScreenPlaylistButtonCommand);
            ApplicationCommands.NowPlayingScreenLyricsButtonCommand.RegisterCommand(this.NowPlayingScreenLyricsButtonCommand);
            ApplicationCommands.NowPlayingScreenArtistInformationButtonCommand.RegisterCommand(this.NowPlayingScreenArtistInformationButtonCommand);
        }
        #endregion

        #region Private
        private void SetShowCase()
        {
            this.SlideDirection = SlideDirection.LeftToRight;
            this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenShowcase).FullName);

            isShowCaseVisible = true;
            isPlaylistVisible = false;
            isLyricsVisible = false;
            isArtistInformationVisible = false;
        }

        private void SetPlaylist()
        {
            this.SlideDirection = SlideDirection.LeftToRight;
            if (isShowCaseVisible) this.SlideDirection = SlideDirection.RightToLeft;
            this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenPlaylist).FullName);

            isShowCaseVisible = false;
            isPlaylistVisible = true;
            isLyricsVisible = false;
            isArtistInformationVisible = false;
        }

        private void SetLyrics()
        {
            this.SlideDirection = SlideDirection.RightToLeft;
            if (isArtistInformationVisible) this.SlideDirection = SlideDirection.LeftToRight;
            this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenLyrics).FullName);

            isShowCaseVisible = false;
            isPlaylistVisible = false;
            isLyricsVisible = true;
            isArtistInformationVisible = false;
        }

        private void SetArtistInformation()
        {
            this.SlideDirection = SlideDirection.RightToLeft;
            this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenArtistInformation).FullName);

            isShowCaseVisible = false;
            isPlaylistVisible = false;
            isLyricsVisible = false;
            isArtistInformationVisible = true;
        }

        private void SetNowPlaying()
        {
            if (this.playbackService.Queue.Count > 0)
            {
                if (isShowCaseVisible)
                {
                    this.SetShowCase();
                }
                else if (isPlaylistVisible)
                {
                    this.SetPlaylist();
                }
                else if (isArtistInformationVisible)
                {
                    this.SetArtistInformation();
                }
            }
            else
            {
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
