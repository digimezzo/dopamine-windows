using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingShowcaseViewModel : ContextMenuViewModelBase
    {
        public NowPlayingShowcaseViewModel(IUnityContainer container) : base(container)
        {
        }

        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
    }
}
