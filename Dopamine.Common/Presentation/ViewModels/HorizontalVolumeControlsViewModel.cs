using Microsoft.Practices.ServiceLocation;
using Dopamine.Common.Services.Playback;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class HorizontalVolumeControlsViewModel : VolumeControlsViewModel
    {
        #region Construction
        public HorizontalVolumeControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
        #endregion
    }
}
