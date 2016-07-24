using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PopupVolumeControlsViewModel : VolumeControlsViewModel
    {
        #region Construction
        public PopupVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
        #endregion
    }
}
