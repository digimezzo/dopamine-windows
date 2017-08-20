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
        private ISQLiteConnectionFactory factory;
        private ILogClient logClient;
        #endregion

        #region Properties
        public ISQLiteConnectionFactory Factory => this.factory;
        #endregion

        #region Construction
        public GenreRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }
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
                            LogClient.Current.Error("There was a problem while deleting orphaned Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public abstract Task<Genre> AddGenreAsync(Genre genre);

        public abstract Task<Genre> GetGenreAsync(string genreName);

        public abstract Task<List<Genre>> GetGenresAsync();
        #endregion
    }
}
