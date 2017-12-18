using SQLite;

namespace Dopamine.Data.Contracts
{
    public interface ISQLiteConnectionFactory
    {
        string DatabaseFile { get; }
        SQLiteConnection GetConnection();
    }
}
