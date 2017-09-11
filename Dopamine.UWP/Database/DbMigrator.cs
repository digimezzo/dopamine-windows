using Dopamine.Core.Database;
using System.IO;

namespace Dopamine.UWP.Database
{
    public class DbMigrator : Core.Database.DbMigrator
    {
        #region Construction
        public DbMigrator(ISQLiteConnectionFactory factory) : base(factory)
        {
        }
        #endregion

        #region Overrides
        protected override void BackupDatabase()
        {
           // No implementation required
        }
        #endregion
    }
}
