using Dopamine.ViewModels;
using Dopamine.Services.Playback;
using CommonServiceLocator;

namespace Dopamine.ViewModels.Common
{
    public class PopupVolumeControlsViewModel : VolumeControlsViewModel
    {
        public PopupVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}
