using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Playback;
using CommonServiceLocator;

namespace Dopamine.ViewModels.Common
{
    public class ProgressControlsThinViewModel : ProgressControlsViewModel
    {
        public ProgressControlsThinViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
    }
}