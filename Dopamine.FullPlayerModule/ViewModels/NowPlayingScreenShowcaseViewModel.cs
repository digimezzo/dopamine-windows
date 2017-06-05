using Dopamine.Common.Presentation.ViewModels.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenShowcaseViewModel : ContextMenuViewModelBase
    {
        #region Construction
        public NowPlayingScreenShowcaseViewModel(IUnityContainer container) : base(container)
        {
        }
        #endregion

        #region Overrides
        protected override void SearchOnline(string id)
        {
            // No implementation required here
        }
        #endregion
    }
}