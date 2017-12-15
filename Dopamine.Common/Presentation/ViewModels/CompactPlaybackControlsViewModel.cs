using Dopamine.Presentation.ViewModels;
using Dopamine.Services.Contracts.Playback;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CompactPlaybackControlsViewModel : PlaybackControlsViewModel
    {
        public CompactPlaybackControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>(), ServiceLocator.Current.GetInstance<IEventAggregator>())
        {
        }
    }
}
