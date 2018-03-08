using Dopamine.ViewModels.Common.Base;
using Unity;

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
