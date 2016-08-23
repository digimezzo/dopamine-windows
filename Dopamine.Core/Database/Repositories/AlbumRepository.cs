using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using SQLite.Net;
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            string q = "SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                       " WHERE (tra.ArtistID IN (?) OR alb.AlbumArtist IN (?)) AND fol.ShowInCollection=1;";
                            
                            albums = conn.Query<Album>(q, Utils.ToQueryList(artists.Select((a) => a.ArtistID).ToList()), Utils.ToQueryList(artists.Select((a) => a.ArtistName).ToList()));
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Albums for Artists. Exception: {0}", ex.Message);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            string q = "SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                       " WHERE tra.GenreID IN (?) AND fol.ShowInCollection=1;";

                            albums = conn.Query<Album>(q, Utils.ToQueryList(genres.Select((g) => g.GenreID).ToList()));
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Albums for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            albums = conn.Query<Album>("SELECT Album.AlbumID, Album.AlbumTitle, Album.AlbumArtist, Album.Year, Album.ArtworkID, Album.DateLastSynced, Album.DateAdded, MAX(Track.DateLastPlayed) AS maxdatelastplayed, SUM(Track.PlayCount) AS playcountsum FROM Album" +
                                                       " LEFT JOIN Track ON Album.AlbumID = Track.AlbumID" +
                                                       " WHERE Track.PlayCount IS NOT NULL AND Track.PlayCount > 0" +
                                                       " GROUP BY Album.AlbumID" +
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });
        }
        #endregion
    }
}
