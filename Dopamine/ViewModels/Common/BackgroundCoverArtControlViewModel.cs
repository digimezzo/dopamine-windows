using Dopamine.Services.Appearance;
using Dopamine.Services.Cache;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;

namespace Dopamine.ViewModels.Common
{
    public class BackgroundCoverArtControlViewModel : CoverArtControlViewModel
    {
        private IAppearanceService appearanceService;
        private ICacheService cacheService;
        private IMetadataService metadataService;
        
        public BackgroundCoverArtControlViewModel(IPlaybackService playbackService,
            ICacheService cacheService, IAppearanceService appearanceService, 
            IMetadataService metadataService) : base(playbackService, cacheService, metadataService)
        {
            this.playbackService = playbackService;
            this.appearanceService = appearanceService;
            this.cacheService = cacheService;
            this.metadataService = metadataService;
        }
    }
}
