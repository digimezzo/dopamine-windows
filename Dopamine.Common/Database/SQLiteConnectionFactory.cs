using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using System.IO;

namespace Dopamine.Common.Database
{
    public class SQLiteConnectionFactory : Core.Database.SQLiteConnectionFactory
    {
        #region Construction
        public SQLiteConnectionFactory()
        {
            var migrator = new DbMigrator(this);
            migrator.Initialize();
        }
        #endregion

        #region Overrides
        public override string DatabaseFile => Path.Combine(SettingsClient.ApplicationFolder(), ProductInformation.ApplicationName + ".db");
        #endregion
    }
}
