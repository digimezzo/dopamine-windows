using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using SQLite;

namespace Dopamine.Common.Database
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
            this.databaseFile = System.IO.Path.Combine(SettingsClient.ApplicationFolder(), ProductInformation.ApplicationAssemblyName + ".db");
        }
        #endregion

        #region Public
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(this.databaseFile) { BusyTimeout = new System.TimeSpan(0,0,1)};
        }
        #endregion
    }
}
