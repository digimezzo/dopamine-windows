using Dopamine.Core.Base;
using SQLite;

namespace Dopamine.Core.Database
{
    public abstract class SQLiteConnectionFactory : ISQLiteConnectionFactory
    {
        #region ISQLiteConnectionFactory
        public abstract string DatabaseFile { get; }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(this.DatabaseFile);
        }
        #endregion
    }
}
