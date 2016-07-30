using Dopamine.Core.Prism;
using Dopamine.SettingsModule.ViewModels;
using Dopamine.SettingsModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.SettingsModule
{
    public class SettingsModule : IModule
    {

        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public SettingsModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            this.container.RegisterType<object, SettingsViewModel>(typeof(SettingsViewModel).FullName);
            this.container.RegisterType<object, Settings>(typeof(Settings).FullName);
            this.container.RegisterType<object, SettingsSubMenu>(typeof(SettingsSubMenu).FullName);

            this.container.RegisterType<object, SettingsCollectionFoldersViewModel>(typeof(SettingsCollectionFoldersViewModel).FullName);
            this.container.RegisterType<object, SettingsCollectionFolders>(typeof(SettingsCollectionFolders).FullName);
            this.container.RegisterType<object, SettingsCollectionViewModel>(typeof(SettingsCollectionViewModel).FullName);
            this.container.RegisterType<object, SettingsCollection>(typeof(SettingsCollection).FullName);
            this.container.RegisterType<object, SettingsAppearanceThemeViewModel>(typeof(SettingsAppearanceThemeViewModel).FullName);
            this.container.RegisterType<object, SettingsAppearanceTheme>(typeof(SettingsAppearanceTheme).FullName);
            this.container.RegisterType<object, SettingsAppearanceLanguageViewModel>(typeof(SettingsAppearanceLanguageViewModel).FullName);
            this.container.RegisterType<object, SettingsAppearanceLanguage>(typeof(SettingsAppearanceLanguage).FullName);
            this.container.RegisterType<object, SettingsAppearanceViewModel>(typeof(SettingsAppearanceViewModel).FullName);
            this.container.RegisterType<object, SettingsAppearance>(typeof(SettingsAppearance).FullName);
            this.container.RegisterType<object, SettingsBehaviourViewModel>(typeof(SettingsBehaviourViewModel).FullName);
            this.container.RegisterType<object, SettingsBehaviour>(typeof(SettingsBehaviour).FullName);
            this.container.RegisterType<object, FolderViewModel>(typeof(FolderViewModel).FullName);
            this.container.RegisterType<object, SettingsPlaybackViewModel>(typeof(SettingsPlaybackViewModel).FullName);
            this.container.RegisterType<object, SettingsPlayback>(typeof(SettingsPlayback).FullName);
            this.container.RegisterType<object, SettingsStartupViewModel>(typeof(SettingsStartupViewModel).FullName);
            this.container.RegisterType<object, SettingsStartup>(typeof(SettingsStartup).FullName);

            this.regionManager.RegisterViewWithRegion(RegionNames.SettingsRegion, typeof(Views.SettingsCollection));
        }
        #endregion
    }
}
