using SQLite;

namespace Dopamine.Common.Database
{
    public interface ISQLiteConnectionFactory
    {
        string DatabaseFile { get; }
        SQLiteConnection GetConnection();
    }
}
