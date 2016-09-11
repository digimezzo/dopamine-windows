using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Settings;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class BackgroundCoverArtControlViewModel : CoverArtControlViewModel
    {
        #region Variables
        private IAppearanceService appearanceService;
        private double opacity;
        private Album previousAlbum;
        private Album album;
        #endregion

        #region Properties
        public double Opacity
        {
            get { return this.opacity; }
            set { SetProperty<double>(ref this.opacity, value); }
        }
        #endregion

        #region Construction
        public BackgroundCoverArtControlViewModel(IPlaybackService playbackService, IAppearanceService appearanceService) : base(playbackService)
        {

            this.playbackService = playbackService;
            this.appearanceService = appearanceService;

            this.appearanceService.ThemeChanged += (_, __) => this.Opacity = XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme") ? 1.0 : 0.5;

            this.Opacity = XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme") ? 1.0 : 0.5;
        }
        #endregion

        #region Overrides
        protected override void ShowCoverArtAsync(TrackInfo trackInfo)
        {
            if (trackInfo != null)
            {
                this.previousAlbum = this.album;
                this.album = new Album
                {
                    AlbumArtist = this.playbackService.PlayingTrack.AlbumArtist,
                    AlbumTitle = this.playbackService.PlayingTrack.AlbumTitle,
                    Year = this.playbackService.PlayingTrack.AlbumYear,
                    ArtworkID = this.playbackService.PlayingTrack.AlbumArtworkID,
                };

                if (this.album != this.previousAlbum) base.ShowCoverArtAsync(trackInfo);
            }
        }
        #endregion
    }

}
