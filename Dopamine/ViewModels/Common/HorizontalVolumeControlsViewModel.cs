using Dopamine.Presentation.ViewModels;
using Dopamine.Services.Contracts.Playback;
using CommonServiceLocator;

namespace Dopamine.ViewModels.Common
{
    public class HorizontalVolumeControlsViewModel : VolumeControlsViewModel
    {
        public HorizontalVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}