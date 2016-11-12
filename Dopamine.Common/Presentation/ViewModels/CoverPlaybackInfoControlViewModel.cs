using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Database.Repositories.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CoverPlaybackInfoControlViewModel : PlaybackInfoControlViewModel
    {
        #region Construction
        public CoverPlaybackInfoControlViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>(), ServiceLocator.Current.GetInstance<IMetadataService>(),ServiceLocator.Current.GetInstance<ITrackRepository>())
        {
        }
        #endregion
    }
}
