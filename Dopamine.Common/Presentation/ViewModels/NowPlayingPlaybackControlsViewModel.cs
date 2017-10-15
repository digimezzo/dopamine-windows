using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        private bool isShowCaseSelected;
        private bool isPlaylistSelected;
        private bool isLyricsSelected;
        private bool isArtistInformationSelected;

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
            get { return this.PlaybackService.Queue.Count > 0; }
        }

        public NowPlayingPlaybackControlsViewModel(IUnityContainer container) : base(container)
        {
            this.PlaybackService.PlaybackSuccess += (_) => RaisePropertyChanged(nameof(this.HasPlaybackQueue));

            this.PlaybackService.PlaybackStopped += (_, __) =>
            {
                this.Reset();
            };

            this.SelectMenuItem();
        }

        private void SelectMenuItem()
        {
            if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                SelectedNowPlayingPage page = (SelectedNowPlayingPage)SettingsClient.Get<int>("FullPlayer", "SelectedNowPlayingPage");

                switch (page)
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
    }
}
