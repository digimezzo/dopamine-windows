using Dopamine.ControlsModule.ViewModels;
using Dopamine.ControlsModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.ControlsModule
{
    public class ControlsModule : IModule
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public ControlsModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            // Register Views and ViewModels with UnityContainer
            this.container.RegisterType<object, SearchControl>(typeof(SearchControl).FullName);
            this.container.RegisterType<object, PlayAllControl>(typeof(PlayAllControl).FullName);
            this.container.RegisterType<object, PlayAllControlViewModel>(typeof(PlayAllControlViewModel).FullName);
            this.container.RegisterType<object, ShuffleAllControl>(typeof(ShuffleAllControl).FullName);
            this.container.RegisterType<object, ShuffleAllControlViewModel>(typeof(ShuffleAllControlViewModel).FullName);
            this.container.RegisterType<object, NothingPlayingControl>(typeof(NothingPlayingControl).FullName);
            this.container.RegisterType<object, NothingPlayingControlViewModel>(typeof(NothingPlayingControlViewModel).FullName);
        }
        #endregion
    }
}
