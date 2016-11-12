using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class QueuedTrackRepository : IQueuedTrackRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public QueuedTrackRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IQueuedTrackRepository
        public async Task<List<string>> GetSavedQueuedPathsAsync()
        {
            var paths = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            paths = conn.Table<QueuedTrack>().ToList().Select((t)=>t.Path).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get queued paths. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return paths;
        }

        public async Task SaveQueuedPathsAsync(IList<string> paths)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // First, clear old queued tracks
                            conn.Execute("DELETE FROM QueuedTrack;");

                            // Then, insert new queued tracks (using a common transaction speeds up inserts and updates)
                            conn.Execute("BEGIN TRANSACTION;");

                            foreach (string path in paths)
                            {
                                conn.Execute("INSERT INTO QueuedTrack(OrderID,Path,SafePath) VALUES(0,?,?);", path, path.ToSafePath());
                            }

                            conn.Execute("UPDATE QueuedTrack SET OrderID=QueuedTrackID;");

                            conn.Execute("COMMIT;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not save queued paths. Exception: {0}", ex.Message);
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
