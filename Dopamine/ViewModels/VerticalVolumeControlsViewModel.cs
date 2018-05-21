using Dopamine.Services.Contracts.Playback;
using CommonServiceLocator;

namespace Dopamine.ViewModels
{
    public class VerticalVolumeControlsViewModel : VolumeControlsViewModel
    {
        // Workaround to have inheritance with dependency injection
        public VerticalVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}