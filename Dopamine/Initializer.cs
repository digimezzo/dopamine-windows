using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Data;
using System;
using System.Threading.Tasks;

namespace Dopamine
{
    public class Initializer
    {
        private DbMigrator migrator;

        public Initializer()
        {
            this.migrator = new DbMigrator(new SQLiteConnectionFactory());
        }

        public bool IsMigrationNeeded()
        {
            bool issettingsMigrationNeeded = SettingsClient.IsMigrationNeeded();
            bool isDatabaseMigrationNeeded = this.migrator.IsMigrationNeeded();

            return issettingsMigrationNeeded | isDatabaseMigrationNeeded;
        }

        public async Task MigrateAsync()
        {
            // Migrate settings
            await this.MigrateSettingsAsync();

            // Migrate database
            await this.MigrateDatabaseAsync();
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
                if (this.migrator.IsMigrationNeeded())
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
