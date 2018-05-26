using CommonServiceLocator;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using Dopamine.Services.Scrobbling;

namespace Dopamine.ViewModels.Common
{
    public class CoverPlaybackInfoControlViewModel : PlaybackInfoControlViewModel
    {
        public CoverPlaybackInfoControlViewModel() : base(
            ServiceLocator.Current.GetInstance<IPlaybackService>(), 
            ServiceLocator.Current.GetInstance<IMetadataService>(),
            ServiceLocator.Current.GetInstance<IScrobblingService>())
        {
        }
    }
}