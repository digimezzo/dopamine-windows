namespace Dopamine.Core.Database
{
    public interface IDbMigrator
    {
        ISQLiteConnectionFactory Factory { get; }

        bool DatabaseNeedsUpgrade();

        void InitializeNewDatabase();

        void UpgradeDatabase();

        bool DatabaseExists();
    }
}
