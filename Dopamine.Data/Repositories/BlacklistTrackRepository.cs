using Digimezzo.Foundation.Core.Logging;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class BlacklistTrackRepository : IBlacklistTrackRepository
    {
        private ISQLiteConnectionFactory factory;

        public BlacklistTrackRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task AddToBlacklistAsync(IList<BlacklistTrack> tracks)
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

        public async Task RemoveFromBlacklistAsync(long blacklistTrackId)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"DELETE FROM BlacklistTrack WHERE BlacklistTrackID={blacklistTrackId};");

                            LogClient.Info("Removed the track with BlacklistTrackID={0}", blacklistTrackId);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not remove the track from blacklist with BlacklistTrackID={0}. Exception: {1}", blacklistTrackId, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task RemoveAllFromBlacklistAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"DELETE FROM BlacklistTrack;");
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

        public async Task<bool> IsInBlacklistAsync(string safePath) {
            long numberInBlacklist = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            numberInBlacklist = conn.ExecuteScalar<long>($"SELECT COUNT(BlacklistTrackID) FROM BlacklistTrack WHERE SafePath='{DataUtils.EscapeQuotes(safePath)}';");
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

        public async Task<IList<BlacklistTrack>> GetBlacklistTracksAsync()
        {
            var allBacklistTracks = new List<BlacklistTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            allBacklistTracks = conn.Table<BlacklistTrack>().Select((s) => s).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the blacklist tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }

            });

            return allBacklistTracks;
        }
    }
}
