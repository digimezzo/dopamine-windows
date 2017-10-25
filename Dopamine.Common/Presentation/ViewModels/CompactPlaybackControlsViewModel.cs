using Dopamine.Common.Services.Playback;
using Prism.Events;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CompactPlaybackControlsViewModel : PlaybackControlsViewModel
    {
        public CompactPlaybackControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>(), ServiceLocator.Current.GetInstance<IEventAggregator>())
        {
        }
    }
}
