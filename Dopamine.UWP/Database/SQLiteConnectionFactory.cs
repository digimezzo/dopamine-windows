using Dopamine.UWP.Base;
using Dopamine.UWP.IO;
using System.IO;

namespace Dopamine.UWP.Database
{
    public class SQLiteConnectionFactory : Core.Database.SQLiteConnectionFactory
    {
        #region Overrides
        public override string DatabaseFile => Path.Combine(LegacyPaths.LocalAppDataFolder(), ProductInformation.ApplicationName + ".db");
        #endregion
    }
}
