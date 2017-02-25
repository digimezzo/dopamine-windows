using Dopamine.Common.Base;
using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;

namespace Dopamine.MiniPlayerModule.ViewModels
{
    public class MiniPlayerPlaylistViewModel : NowPlayingViewModelBase
   {
      #region Construction
      public MiniPlayerPlaylistViewModel(IUnityContainer container)
         : base(container)
      {
      }
      #endregion

      #region Overrides
      protected override async Task LoadedCommandAsync()
      {
            if (!this.IsFirstLoad()) return;
            await Task.Delay(Constants.MiniPlayerListLoadDelay); // Wait for the UI to slide in
            await this.FillListsAsync(); // Fill all the lists
      }

      protected override void Subscribe()
      {
         // Not required here
      }

      protected override void Unsubscribe()
      {
         // Not required here
      }
      #endregion
   }
}
