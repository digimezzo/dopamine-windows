using Dopamine.Services.Playback;
using Dopamine.Services.Playback;
using CommonServiceLocator;

namespace Dopamine.ViewModels.Common
{
    public class CoverPlaybackInfoControlViewModel : PlaybackInfoControlViewModel
    {
        public CoverPlaybackInfoControlViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}