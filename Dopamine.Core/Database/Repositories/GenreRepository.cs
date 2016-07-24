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
        #region IGenreRepository
        public async Task<List<Genre>> GetGenresAsync()
        {
            var genres = new List<Genre>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            genres = (from gen in db.Genres
                                      join tra in db.Tracks on gen.GenreID equals tra.GenreID
                                      join fol in db.Folders on tra.FolderID equals fol.FolderID
                                      where fol.ShowInCollection == 1
                                      select gen).Distinct().ToList();

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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            genre = db.Genres.Select((g) => g).Where((g) => g.GenreName.Equals(genreName)).FirstOrDefault();
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.Genres.Add(genre);
                            db.SaveChanges();
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.Genres.RemoveRange(db.Genres.Where((g) => !db.Tracks.Select((t) => t.GenreID).Distinct().Contains(g.GenreID)));
                            db.SaveChanges();
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
