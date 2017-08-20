using SQLite;

namespace Dopamine.Core.Database
{
    public interface ISQLiteConnectionFactory
    {
        string DatabaseFile { get; }
        SQLiteConnection GetConnection();
    }
}
