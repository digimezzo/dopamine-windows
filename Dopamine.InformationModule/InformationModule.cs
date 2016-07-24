using Dopamine.Core.Prism;
using Dopamine.InformationModule.ViewModels;
using Dopamine.InformationModule.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.InformationModule
{
    public class InformationModule : IModule
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public InformationModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            this.container.RegisterType<object, InformationViewModel>(typeof(InformationViewModel).FullName);
            this.container.RegisterType<object, Information>(typeof(Information).FullName);
            this.container.RegisterType<object, InformationAboutViewModel>(typeof(InformationAboutViewModel).FullName);
            this.container.RegisterType<object, InformationAbout>(typeof(InformationAbout).FullName);
            this.container.RegisterType<object, InformationHelpViewModel>(typeof(InformationHelpViewModel).FullName);
            this.container.RegisterType<object, InformationHelp>(typeof(InformationHelp).FullName);
            this.container.RegisterType<object, InformationSubMenu>(typeof(InformationSubMenu).FullName);
            this.container.RegisterType<object, InformationAboutLicense>(typeof(InformationAboutLicense).FullName);

            this.regionManager.RegisterViewWithRegion(RegionNames.InformationRegion, typeof(InformationHelp));
        }
        #endregion
    }
}
