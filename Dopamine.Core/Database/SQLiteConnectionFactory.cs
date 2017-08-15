using Dopamine.Core.Base;
using Dopamine.Core.IO;
using SQLite;

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
            this.databaseFile = System.IO.Path.Combine(Storage.StorageFolder, ProductInformationBase.ApplicationName + ".db");
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
