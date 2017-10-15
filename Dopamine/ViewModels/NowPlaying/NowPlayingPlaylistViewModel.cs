using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingPlaylistViewModel : NowPlayingViewModelBase
    {
        public NowPlayingPlaylistViewModel(IUnityContainer container) : base(container)
        {
        }

        protected async override Task LoadedCommandAsync()
        {
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
        }
    }
}
