using Dopamine.ControlsModule.Views;
using Dopamine.Common.Prism;
using Dopamine.MiniPlayerModule.ViewModels;
using Dopamine.MiniPlayerModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.MiniPlayerModule
{
    public class MiniPlayerModule : IModule
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public MiniPlayerModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            // Register Views and ViewModels with UnityContainer
            this.container.RegisterType<object, CoverPlayer>(typeof(CoverPlayer).FullName);
            this.container.RegisterType<object, CoverPlayerViewModel>(typeof(CoverPlayerViewModel).FullName);
            this.container.RegisterType<object, MicroPlayer>(typeof(MicroPlayer).FullName);
            this.container.RegisterType<object, MicroPlayerViewModel>(typeof(MicroPlayerViewModel).FullName);
            this.container.RegisterType<object, MiniPlayerPlaylist>(typeof(MiniPlayerPlaylist).FullName);
            this.container.RegisterType<object, NanoPlayer>(typeof(NanoPlayer).FullName);
            this.container.RegisterType<object, NanoPlayerViewModel>(typeof(NanoPlayerViewModel).FullName);
        }
        #endregion
    }
}
