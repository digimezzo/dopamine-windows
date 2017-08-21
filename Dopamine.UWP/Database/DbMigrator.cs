using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using System;
using System.IO;

namespace Dopamine.UWP.Database
{
    public class DbMigrator : Core.Database.DbMigrator
    {
        #region Construction
        public DbMigrator(ISQLiteConnectionFactory factory) : base(factory)
        {
        }
        #endregion

        #region Overrides
        public override void UpgradeDatabase()
        {
            // Create a copy of the database file
            try
            {
                string databaseFileCopy = this.Factory.DatabaseFile + ".old";

                if (File.Exists(databaseFileCopy)) File.Delete(databaseFileCopy);
                File.Copy(this.Factory.DatabaseFile, databaseFileCopy);
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Info("Could not create a copy of the database file. Exception: {0}", ex.Message);
            }

            base.UpgradeDatabase();
        }

        public override bool DatabaseExists()
        {
            return File.Exists(this.Factory.DatabaseFile);
        }
        #endregion
    }
}
