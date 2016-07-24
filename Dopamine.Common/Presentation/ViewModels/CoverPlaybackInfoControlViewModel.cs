using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CoverPlaybackInfoControlViewModel : PlaybackInfoControlViewModel
    {
        #region Construction
        public CoverPlaybackInfoControlViewModel() : base(ServiceLocator.Current.GetInstance<IPlaybackService>())
        {
        }
        #endregion
    }
}
