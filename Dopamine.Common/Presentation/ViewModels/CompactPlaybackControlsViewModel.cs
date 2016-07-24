using Dopamine.Common.Services.Playback;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CompactPlaybackControlsViewModel : PlaybackControlsViewModel
    {
        #region Construction
        public CompactPlaybackControlsViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>(), ServiceLocator.Current.GetInstance<IEventAggregator>())
        {
        }
        #endregion

    }
}
