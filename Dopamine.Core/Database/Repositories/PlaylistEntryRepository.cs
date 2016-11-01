using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using System;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class PlaylistEntryRepository : IPlaylistEntryRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public PlaylistEntryRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IPlaylistEntryRepository
        public async Task DeleteOrphanedPlaylistEntriesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("DELETE FROM PlaylistEntry WHERE TrackID NOT IN (SELECT TrackID FROM Track);");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("There was a problem while deleting orphaned PlaylistEntries. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }
        #endregion
    }
}
