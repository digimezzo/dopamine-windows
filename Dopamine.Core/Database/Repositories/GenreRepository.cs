using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public abstract class GenreRepository : IGenreRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Properties
        public SQLiteConnectionFactory Factory => this.factory;
        #endregion

        #region IGenreRepository
        public async Task DeleteOrphanedGenresAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("DELETE FROM Genre WHERE GenreID NOT IN (SELECT GenreID FROM Track);");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("There was a problem while deleting orphaned Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public abstract Task<Genre> AddGenreAsync(Genre genre);

        public abstract Task<Genre> GetGenreAsync(string genreName);

        public abstract Task<List<Genre>> GetGenresAsync();
        #endregion
    }
}
