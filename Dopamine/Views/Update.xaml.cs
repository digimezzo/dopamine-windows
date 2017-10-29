using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Controls;
using Dopamine.Common.Database;
using System;
using System.Threading.Tasks;

namespace Dopamine.Views
{
    public partial class Update : DopamineWindow
    {
        public Update()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PerformUpdateAsync();
        }

        private async void PerformUpdateAsync()
        {
            // Migrate settings
            await this.MigrateSettingsAsync();

            // Migrate database
            await this.MigrateDatabaseAsync();

            // Start the bootstrapper
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
            this.Close();
        }

        private async Task MigrateSettingsAsync()
        {
            try
            {
                if (SettingsClient.IsMigrationNeeded())
                {
                    LogClient.Info("Migrating settings");
                    await Task.Run(() => SettingsClient.Migrate());
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem migrating the settings. Exception: {0}", ex.Message);
            }
           
        }

        private async Task MigrateDatabaseAsync()
        {
            try
            {
                var migrator = new DbMigrator(new SQLiteConnectionFactory());

                if (migrator.IsMigrationNeeded())
                {
                    LogClient.Info("Migrating database");
                    await Task.Run(() => migrator.Migrate());
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem migrating the database. Exception: {0}", ex.Message);
            }
        }
    }
}
