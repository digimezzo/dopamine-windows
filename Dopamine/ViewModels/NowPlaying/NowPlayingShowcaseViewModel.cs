using Dopamine.ViewModels.Common.Base;
using Prism.Ioc;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingShowcaseViewModel : ContextMenuViewModelBase
    {
        public NowPlayingShowcaseViewModel(IContainerProvider container) : base(container)
        {
        }

        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
    }
}
