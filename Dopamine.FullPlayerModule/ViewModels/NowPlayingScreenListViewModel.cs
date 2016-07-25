using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Base;
using System.Threading.Tasks;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenListViewModel : NowPlayingViewModel
    {
        #region Construction
        public NowPlayingScreenListViewModel() : base()
        {
        }
        #endregion

        #region Overrides
        protected async override Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }
        #endregion
    }
}
