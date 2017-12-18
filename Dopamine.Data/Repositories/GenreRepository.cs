using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private ISQLiteConnectionFactory factory;
        private ILocalizationInfo info;

        public GenreRepository(ISQLiteConnectionFactory factory, ILocalizationInfo info)
        {
            this.factory = factory;
            this.info = info;
        }

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
                            genres = conn.Query<Genre>("SELECT DISTINCT gen.GenreID, " +
                                                       $"REPLACE(gen.GenreName,'{Defaults.UnknownGenreText}','{this.info.UnknownGenreText}') GenreName FROM Genre gen " +
                                                       "INNER JOIN Track tra ON gen.GenreID=tra.GenreID " +
                                                       "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                       "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID " +
                                                       "WHERE fol.ShowInCollection=1");

                            // Orders the Genres
                            genres = genres.OrderBy((g) => DatabaseUtils.GetSortableString(g.GenreName)).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
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
                            LogClient.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get the Genre with GenreName='{0}'. Exception: {1}", genreName, ex.Message);
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
                            long maxGenreID = conn.ExecuteScalar<long>("SELECT MAX(GenreID) FROM Genre;");
                            genre.GenreID = maxGenreID + 1;
                            conn.Insert(genre);
                        }
                        catch (Exception ex)
                        {
                            genre = null;
                            LogClient.Error("Could not create the Genre with GenreName='{0}'. Exception: {1}", genre.GenreName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
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
    }
}
