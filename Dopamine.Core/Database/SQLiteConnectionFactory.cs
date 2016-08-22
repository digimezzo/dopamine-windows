using Dopamine.Core.Base;
using Dopamine.Core.IO;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace Dopamine.Core.Database
{
    public class SQLiteConnectionFactory
    {
        #region Private
        private string databaseFile;
        #endregion

        #region ReadOnly Properties
        public string DatabaseFile
        {
            get { return this.databaseFile; }
        }
        #endregion

        #region Construction
        public SQLiteConnectionFactory()
        {
            this.databaseFile = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName + ".db");
        }
        #endregion

        #region Public
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(new SQLitePlatformWin32(), this.databaseFile);
        }
        #endregion
    }
}
