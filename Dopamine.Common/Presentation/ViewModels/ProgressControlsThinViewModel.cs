using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ProgressControlsThinViewModel : ProgressControlsViewModel
    {
        public ProgressControlsThinViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}