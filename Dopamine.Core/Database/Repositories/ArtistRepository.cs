using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public abstract class ArtistRepository : IArtistRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Properties
        public SQLiteConnectionFactory Factory => this.factory;
        #endregion

        #region Construction
        public ArtistRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IArtistRepository
        public async Task DeleteOrphanedArtistsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("DELETE FROM Artist WHERE ArtistID NOT IN (SELECT ArtistID FROM Track);");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("There was a problem while deleting orphaned Artists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public abstract Task<Artist> AddArtistAsync(Artist artist);

        public abstract Task<Artist> GetArtistAsync(string artistName);

        public abstract Task<List<Artist>> GetArtistsAsync(ArtistOrder artistOrder);

        #endregion
    }
}
