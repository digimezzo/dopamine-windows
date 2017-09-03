using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories
{
    public class GenreRepository : Core.Database.Repositories.GenreRepository
    {
        #region Variables
        private ILocalizationInfo info;
        #endregion

        #region Construction
        public GenreRepository(ISQLiteConnectionFactory factory, ILocalizationInfo info) : base(factory, info)
        {
            this.info = info;
        }
        #endregion

        #region Overrides
        public override async Task<List<Genre>> GetGenresAsync()
        {
            var genres = new List<Genre>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            genres = conn.Query<Genre>("SELECT DISTINCT gen.GenreID, " +
                                                       $"REPLACE(gen.GenreName,'{Defaults.UnknownGenreText}','{this.info.UnknownGenreText}') GenreName FROM Genre gen " +
                                                       "INNER JOIN Track tra ON gen.GenreID=tra.GenreID " +
                                                       "INNER JOIN Folder fol ON tra.FolderID=fol.FolderID " +
                                                       "WHERE fol.ShowInCollection=1");

                            // Orders the Genres
                            genres = genres.OrderBy((g) => DatabaseUtils.GetSortableString(g.GenreName)).ToList();
                        }
                        catch (Exception ex)
                        {
                            CoreLogger.Current.Error("Could not get the Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoreLogger.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return genres;
        }

        public override async Task<Genre> GetGenreAsync(string genreName)
        {
            Genre genre = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            genre = conn.Table<Genre>().Select((g) => g).Where((g) => g.GenreName.Equals(genreName)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            CoreLogger.Current.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoreLogger.Current.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
                }
            });

            return genre;
        }

        public override async Task<Genre> AddGenreAsync(Genre genre)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            long maxGenreID = conn.ExecuteScalar<long>("SELECT MAX(GenreID) FROM Genre;");
                            genre.GenreID = maxGenreID + 1;
                            conn.Insert(genre);
                        }
                        catch (Exception ex)
                        {
                            genre = null;
                            CoreLogger.Current.Error("Could not create the Genre with GenreName='{0}'. Exception: {1}", genre.GenreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoreLogger.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return genre;
        }
        #endregion
    }
}
