using Dopamine.Presentation.ViewModels;
using Dopamine.Services.Contracts.Playback;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PopupVolumeControlsViewModel : VolumeControlsViewModel
    {
        public PopupVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}
