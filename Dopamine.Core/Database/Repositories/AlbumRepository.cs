using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public AlbumRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IAlbumRepository
        public async Task<List<Album>> GetAlbumsAsync()
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            albums = conn.Query<Album>("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                       " WHERE fol.ShowInCollection=1;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get all the Albums. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public async Task<List<Album>> GetAlbumsAsync(IList<Artist> artists)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> artistIDs = artists.Select((a) => a.ArtistID).ToList();
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            string q = string.Format("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                       " WHERE (tra.ArtistID IN ({0}) OR alb.AlbumArtist IN ({1})) AND fol.ShowInCollection=1;", Utils.ToQueryList(artistIDs), Utils.ToQueryList(artistNames));

                            albums = conn.Query<Album>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Albums for Artists. Exception: {0}", ex.Message);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public async Task<List<Album>> GetAlbumsAsync(IList<Genre> genres)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            string q = string.Format("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                                     " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1;", Utils.ToQueryList(genreIDs));

                            albums = conn.Query<Album>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Albums for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public async Task<Album> GetAlbumAsync(string albumTitle, string albumArtist)
        {
            Album album = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            album = conn.Table<Album>().Select((a) => a).Where((a) => a.AlbumTitle.Equals(albumTitle) & a.AlbumArtist.Equals(albumArtist)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Album with AlbumTitle='{0}' and AlbumArtist='{1}'. Exception: {2}", albumTitle, albumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    album = null;
                    LogClient.Instance.Logger.Error("Could not get the Album with AlbumTitle='{0}' and AlbumArtist='{1}'. Exception: {2}", albumTitle, albumArtist, ex.Message);
                }
            });
            return album;
        }

        public async Task<List<Album>> GetAlbumHistoryAsync(int limit)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            albums = conn.Query<Album>("SELECT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded, MAX(tra.DateLastPlayed) AS maxdatelastplayed, SUM(tra.PlayCount) AS playcountsum FROM Album alb" +
                                                       " LEFT JOIN Track tra ON alb.AlbumID = tra.AlbumID" +
                                                       " WHERE tra.PlayCount IS NOT NULL AND tra.PlayCount > 0" +
                                                       " GROUP BY alb.AlbumID" +
                                                       " ORDER BY playcountsum DESC, maxdatelastplayed DESC");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Album history. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public async Task<Album> AddAlbumAsync(Album album)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            long maxAlbumID = conn.ExecuteScalar<long>("SELECT MAX(AlbumID) FROM Album;");
                            album.AlbumID = maxAlbumID + 1;
                            conn.Insert(album);
                        }
                        catch (Exception ex)
                        {
                            album = null;
                            LogClient.Instance.Logger.Error("Could not create the Album with AlbumTitle='{0}' and AlbumArtist='{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return album;
        }

        public async Task<bool> UpdateAlbumAsync(Album album)
        {
            bool isUpdateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Update(album);
                            isUpdateSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not update the Album with Title='{0}' and Album artist = '{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return isUpdateSuccess;
        }

        public async Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID)
        {
            bool isUpdateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            Album dbAlbum = conn.Table<Album>().Select((a) => a).Where((a) => a.AlbumTitle.Equals(albumTitle) & a.AlbumArtist.Equals(albumArtist)).FirstOrDefault();

                            if (dbAlbum != null)
                            {
                                dbAlbum.ArtworkID = artworkID;
                                dbAlbum.DateLastSynced = DateTime.Now.Ticks;
                                conn.Update(dbAlbum);
                                isUpdateSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not update album artwork for album with title '{0}' and album artist '{1}'. Exception: {2}", albumTitle, albumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return isUpdateSuccess;
        }

        public async Task DeleteOrphanedAlbumsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("DELETE FROM Album WHERE AlbumID NOT IN (SELECT AlbumID FROM Track);");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("There was a problem while deleting orphaned Albums. Exception: {0}", ex.Message);
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
