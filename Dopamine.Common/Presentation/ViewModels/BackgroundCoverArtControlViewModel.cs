using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class BackgroundCoverArtControlViewModel : CoverArtControlViewModel
    {
        private IAppearanceService appearanceService;
        private ICacheService cacheService;
        private IMetadataService metadataService;
        private double opacity;
   
        public double Opacity
        {
            get { return this.opacity; }
            set { SetProperty<double>(ref this.opacity, value); }
        }
     
        public BackgroundCoverArtControlViewModel(IPlaybackService playbackService,
            ICacheService cacheService, IAppearanceService appearanceService, 
            IMetadataService metadataService) : base(playbackService, cacheService, metadataService)
        {
            this.playbackService = playbackService;
            this.appearanceService = appearanceService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;

            this.appearanceService.ThemeChanged += useLightTheme => this.Opacity = useLightTheme ? 1.0 : 0.5;

            this.Opacity = SettingsClient.Get<bool>("Appearance", "EnableLightTheme") ? 1.0 : 0.5;
        }
    }
}
