using Digimezzo.Foundation.Core.Logging;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class BlacklistRepository : IBlacklistRepository
    {
        private ISQLiteConnectionFactory factory;

        public BlacklistRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task AddToBlacklistAsync(IList<Blacklist> tracks)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("BEGIN TRANSACTION;");
                            conn.InsertAll(tracks);
                            conn.Execute("COMMIT;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not add blacklist tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<bool> RemoveFromBlacklist(long blacklistId)
        {
            bool result = true;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"DELETE FROM Blacklist WHERE BlacklistID={blacklistId};");

                            LogClient.Info("Removed the track with BlacklistID={0}", blacklistId);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not remove the track from blacklist with BlacklistID={0}. Exception: {1}", blacklistId, ex.Message);
                            result = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                    result = false;
                }
            });

            return result;
        }

        public async Task RemoveAllFromBlacklist()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"DELETE FROM Blacklist;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not delete from blacklist. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<bool> IsInBlacklist(string safePath) {
            long numberInBlacklist = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            numberInBlacklist = conn.ExecuteScalar<long>($"SELECT COUNT(BlacklistID) FROM Blacklist WHERE SafePath='{safePath}';");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get number of tracks in blacklist. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return numberInBlacklist > 0;
        }
    }
}
