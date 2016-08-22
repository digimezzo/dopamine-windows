using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Dopamine.Core.Logging;

namespace Dopamine.Core.Database.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public GenreRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IGenreRepository
        public async Task<List<Genre>> GetGenresAsync()
        {
            var genres = new List<Genre>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            genres = conn.Query<Genre>("SELECT DISTINCT * FROM Genre gen"+
                                                       " INNER JOIN Track tra ON gen.GenreID=tra.GenreID" +
                                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                       " WHERE fol.ShowInCollection=1");

                            // Orders the Genres
                            genres = genres.OrderBy((g) => Utils.GetSortableString(g.GenreName)).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return genres;
        }

        public async Task<Genre> GetGenreAsync(string genreName)
        {
            Genre genre = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            genre = conn.Table<Genre>().Select((g) => g).Where((g) => g.GenreName.Equals(genreName)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
                }
            });

            return genre;
        }

        public async Task<Genre> AddGenreAsync(Genre genre)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Insert(genre);
                        }
                        catch (Exception ex)
                        {
                            genre = null;
                            LogClient.Instance.Logger.Error("Could not create the Genre with GenreName='{0}'. Exception: {1}", genre.GenreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return genre;
        }

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
                            LogClient.Instance.Logger.Error("There was a problem while deleting orphaned Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });
        }
        #endregion
    }
}
