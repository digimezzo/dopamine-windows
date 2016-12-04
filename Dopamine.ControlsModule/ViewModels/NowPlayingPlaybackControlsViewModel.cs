using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Settings;

namespace Dopamine.ControlsModule.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        #region Variables
        private bool isShowCaseSelected;
        private bool isPlaylistSelected;
        private bool isLyricsSelected;
        private bool isArtistInformationSelected;
        #endregion

        #region Properties
        public bool IsShowCaseSelected
        {
            get { return this.isShowCaseSelected; }
            set { SetProperty<bool>(ref this.isShowCaseSelected, value); }
        }

        public bool IsPlaylistSelected
        {
            get { return this.isPlaylistSelected; }
            set { SetProperty<bool>(ref this.isPlaylistSelected, value); }
        }

        public bool IsLyricsSelected
        {
            get { return this.isLyricsSelected; }
            set { SetProperty<bool>(ref this.isLyricsSelected, value); }
        }

        public bool IsArtistInformationSelected
        {
            get { return this.isArtistInformationSelected; }
            set { SetProperty<bool>(ref this.isArtistInformationSelected, value); }
        }

        public bool HasPlaybackQueue
        {
            get { return this.playbackService.Queue.Count > 0; }
        }

        #endregion

        #region Construction
        public NowPlayingPlaybackControlsViewModel(IPlaybackService playbackService) : base(playbackService)
        {
            this.playbackService.PlaybackSuccess += (_) => OnPropertyChanged(() => this.HasPlaybackQueue);

            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
            };

            this.SelectMenuItem();
        }
        #endregion

        #region Private
        private void SelectMenuItem()
        {
            if (XmlSettingsClient.Instance.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                SelectedNowPlayingPage screen = (SelectedNowPlayingPage)XmlSettingsClient.Instance.Get<int>("FullPlayer", "SelectedNowPlayingPage");

                switch (screen)
                {
                    case SelectedNowPlayingPage.ShowCase:
                        this.IsShowCaseSelected = true;
                        break;
                    case SelectedNowPlayingPage.Playlist:
                        this.IsPlaylistSelected = true;
                        break;
                    case SelectedNowPlayingPage.Lyrics:
                        this.IsLyricsSelected = true;
                        break;
                    case SelectedNowPlayingPage.ArtistInformation:
                        this.IsArtistInformationSelected = true;
                        break;
                }
            }
            else
            {
                this.IsPlaylistSelected = true;
            }
        }
        #endregion
    }
}
