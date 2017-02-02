using Dopamine.OobeModule.ViewModels;
using Dopamine.OobeModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.OobeModule
{
    public class OobeModule : IModule
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public OobeModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            // Register Views and ViewModels with UnityContainer
            this.container.RegisterType<object, OobeAppName>(typeof(OobeAppName).FullName);
            this.container.RegisterType<object, OobeAppNameViewModel>(typeof(OobeAppNameViewModel).FullName);
            this.container.RegisterType<object, OobeControls>(typeof(OobeControls).FullName);
            this.container.RegisterType<object, OobeControlsViewModel>(typeof(OobeControlsViewModel).FullName);
            this.container.RegisterType<object, OobeWelcome>(typeof(OobeWelcome).FullName);
            this.container.RegisterType<object, OobeWelcomeViewModel>(typeof(OobeWelcomeViewModel).FullName);
            this.container.RegisterType<object, OobeLanguage>(typeof(OobeLanguage).FullName);
            this.container.RegisterType<object, OobeLanguageViewModel>(typeof(OobeLanguageViewModel).FullName);
            this.container.RegisterType<object, OobeAppearance>(typeof(OobeAppearance).FullName);
            this.container.RegisterType<object, OobeAppearanceViewModel>(typeof(OobeAppearanceViewModel).FullName);
            this.container.RegisterType<object, OobeCollection>(typeof(OobeCollection).FullName);
            this.container.RegisterType<object, OobeCollectionViewModel>(typeof(OobeCollectionViewModel).FullName);
            this.container.RegisterType<object, OobeDonate>(typeof(OobeDonate).FullName);
            this.container.RegisterType<object, OobeDonateViewModel>(typeof(OobeDonateViewModel).FullName);
            this.container.RegisterType<object, OobeFinish>(typeof(OobeFinish).FullName);
            this.container.RegisterType<object, OobeFinishViewModel>(typeof(OobeFinishViewModel).FullName);
        }
        #endregion
    }
}
