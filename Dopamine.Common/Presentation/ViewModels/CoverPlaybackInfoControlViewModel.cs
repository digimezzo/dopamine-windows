using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Playback;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CoverPlaybackInfoControlViewModel : PlaybackInfoControlViewModel
    {
        public CoverPlaybackInfoControlViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}