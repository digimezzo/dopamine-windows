using Dopamine.Core.Database;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using Dopamine.Core.Services.Appearance;
using Dopamine.Core.Settings;
using Dopamine.UWP.Services.Dialog;
using Dopamine.UWP.Views;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<Main>();

            return shell;
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            this.RegisterCoreComponents();
            this.RegisterServices();
            this.RegisterViews();
            this.InitializeDatabase();

            ViewModelLocationProvider.SetDefaultViewModelFactory((type) => { return Container.Resolve(type); });

            return base.OnInitializeAsync(args);
        }

        private void RegisterCoreComponents()
        {
            Container.RegisterSingletonType<ICoreLogger, Logging.CoreLogger>();
            Container.RegisterSingletonType<ISQLiteConnectionFactory, Database.SQLiteConnectionFactory>();
            Container.RegisterSingletonType<IDbMigrator, Database.DbMigrator>();
        }

        private void RegisterServices()
        {
            Container.RegisterSingletonType<IAppearanceService, Services.Appearance.AppearanceService>();
            Container.RegisterSingletonType<IDialogService, DialogService>();
        }

        private void RegisterViews()
        {
            Container.RegisterType<object, InformationAboutLicense>(typeof(InformationAboutLicense).FullName);
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void InitializeDatabase()
        {
            try
            {
                var migrator = new Database.DbMigrator(new Database.SQLiteConnectionFactory());

                if (!migrator.DatabaseExists())
                {
                    // Create the database if it doesn't exist
                    CoreLogger.Current.Info("Creating database");
                    migrator.InitializeNewDatabase();
                }
                else
                {
                    // Upgrade the database if it is not the latest version

                    if (migrator.DatabaseNeedsUpgrade())
                    {
                        CoreLogger.Current.Info("Upgrading database");
                        migrator.UpgradeDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("There was a problem initializing the database. Exception: {0}", ex.Message);
            }

            return;
        }
    }
}
