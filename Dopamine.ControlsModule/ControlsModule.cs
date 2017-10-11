using Dopamine.ControlsModule.Views;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

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
        }
        #endregion
    }
}
