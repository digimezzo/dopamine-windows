using Microsoft.Practices.ServiceLocation;
using Dopamine.Common.Services.Playback;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class HorizontalVolumeControlsViewModel : VolumeControlsViewModel
    {
        public HorizontalVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}