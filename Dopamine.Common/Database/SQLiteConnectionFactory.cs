using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using SQLite;
using System.IO;

namespace Dopamine.Common.Database
{
    public class SQLiteConnectionFactory : ISQLiteConnectionFactory
    {
        #region Construction
        public SQLiteConnectionFactory()
        {
            var migrator = new DbMigrator(this);
            migrator.Initialize();
        }
        #endregion

        #region ISQLiteConnectionFactory
        public string DatabaseFile => Path.Combine(SettingsClient.ApplicationFolder(), ProductInformation.ApplicationName + ".db");
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(this.DatabaseFile);
        }

        
        #endregion
    }
}
