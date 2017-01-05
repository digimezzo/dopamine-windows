using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.ControlsModule.Views;
using Dopamine.Common.Prism;
using Dopamine.FullPlayerModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenViewModel : BindableBase
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

            if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                SelectedNowPlayingPage page = (SelectedNowPlayingPage)SettingsClient.Get<int>("FullPlayer", "SelectedNowPlayingPage");

                switch (page)
                {
                    case SelectedNowPlayingPage.ShowCase:
                        this.isShowCaseVisible = true;
                        break;
                    case SelectedNowPlayingPage.Playlist:
                        this.isPlaylistVisible = true;
                        break;
                    case SelectedNowPlayingPage.Lyrics:
                        this.isLyricsVisible = true;
                        break;
                    case SelectedNowPlayingPage.ArtistInformation:
                        this.isArtistInformationVisible = true;
                        break;
                }
            }
            else
            {
                this.isPlaylistVisible = true;
            }

            this.playbackService.PlaybackSuccess += (_) => this.SetNowPlaying();

            this.NowPlayingScreenShowcaseButtonCommand = new DelegateCommand(() => this.SetShowCase());
            this.NowPlayingScreenPlaylistButtonCommand = new DelegateCommand(() => this.SetPlaylist());
            this.NowPlayingScreenLyricsButtonCommand = new DelegateCommand(() => this.SetLyrics());
            this.NowPlayingScreenArtistInformationButtonCommand = new DelegateCommand(() => this.SetArtistInformation());

            this.LoadedCommand = new DelegateCommand(() => this.SetNowPlaying());

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
            SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingPage", (int) SelectedNowPlayingPage.ShowCase);

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
            SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingPage", (int)SelectedNowPlayingPage.Playlist);

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
            SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingPage", (int)SelectedNowPlayingPage.Lyrics);

            isShowCaseVisible = false;
            isPlaylistVisible = false;
            isLyricsVisible = true;
            isArtistInformationVisible = false;
        }

        private void SetArtistInformation()
        {
            this.SlideDirection = SlideDirection.RightToLeft;
            this.regionManager.RequestNavigate(RegionNames.NowPlayingContentRegion, typeof(NowPlayingScreenArtistInformation).FullName);
            SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingPage", (int)SelectedNowPlayingPage.ArtistInformation);

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
                else if (isLyricsVisible)
                {
                    this.SetLyrics();
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
    }
}
