using SQLite;

namespace Dopamine.Data
{
    public interface ISQLiteConnectionFactory
    {
        string DatabaseFile { get; }
        SQLiteConnection GetConnection();
    }
}
