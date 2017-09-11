namespace Dopamine.Core.Database
{
    public interface IDbMigrator
    {
        ISQLiteConnectionFactory Factory { get; }
    }
}
