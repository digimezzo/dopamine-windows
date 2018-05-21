using Dopamine.ViewModels;
using Dopamine.Services.Contracts.Playback;
using CommonServiceLocator;
using Prism.Events;

namespace Dopamine.ViewModels.Common
{
    public class CompactPlaybackControlsViewModel : PlaybackControlsViewModel
    {
        public CompactPlaybackControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>(), ServiceLocator.Current.GetInstance<IEventAggregator>())
        {
        }
    }
}
