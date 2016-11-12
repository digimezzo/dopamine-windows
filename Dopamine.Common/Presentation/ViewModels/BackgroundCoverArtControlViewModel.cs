using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Settings;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class BackgroundCoverArtControlViewModel : CoverArtControlViewModel
    {
        #region Variables
        private IAppearanceService appearanceService;
        private ICacheService cacheService;
        private double opacity;
        #endregion

        #region Properties
        public double Opacity
        {
            get { return this.opacity; }
            set { SetProperty<double>(ref this.opacity, value); }
        }
        #endregion

        #region Construction
        public BackgroundCoverArtControlViewModel(IPlaybackService playbackService,ICacheService cacheService, IAppearanceService appearanceService, IMetadataService metadataService, ITrackRepository trackRepository) : base(playbackService, cacheService, metadataService, trackRepository)
        {
            this.playbackService = playbackService;
            this.appearanceService = appearanceService;
            this.cacheService = cacheService;

            this.appearanceService.ThemeChanged += (_, __) => this.Opacity = XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme") ? 1.0 : 0.5;

            this.Opacity = XmlSettingsClient.Instance.Get<bool>("Appearance", "EnableLightTheme") ? 1.0 : 0.5;
        }
        #endregion
    }
}
