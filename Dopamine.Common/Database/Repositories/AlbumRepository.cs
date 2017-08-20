using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories
{
    public class AlbumRepository : Core.Database.Repositories.AlbumRepository
    {
        #region Construction
        public AlbumRepository(ISQLiteConnectionFactory factory) : base(factory)
        {
        }
        #endregion

        #region Overrides
        public override async Task<List<Album>> GetAlbumsAsync()
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            albums = conn.Query<Album>("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded, alb.DateCreated FROM Album alb" +
                                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                       " WHERE fol.ShowInCollection=1;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not get all the Albums. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public override async Task<List<Album>> GetAlbumsAsync(IList<Artist> artists)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            List<long> artistIDs = artists.Select((a) => a.ArtistID).ToList();
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            string q = string.Format("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded, alb.DateCreated FROM Album alb" +
                                       " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                       " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                       " WHERE (tra.ArtistID IN ({0}) OR alb.AlbumArtist IN ({1})) AND fol.ShowInCollection=1;", DatabaseUtils.ToQueryList(artistIDs), DatabaseUtils.ToQueryList(artistNames));

                            albums = conn.Query<Album>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not get the Albums for Artists. Exception: {0}", ex.Message);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public override async Task<List<Album>> GetAlbumsAsync(IList<Genre> genres)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            string q = string.Format("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded, alb.DateCreated FROM Album alb" +
                                                     " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1;", DatabaseUtils.ToQueryList(genreIDs));

                            albums = conn.Query<Album>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not get the Albums for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public override async Task<Album> GetAlbumAsync(string albumTitle, string albumArtist)
        {
            Album album = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            album = conn.Table<Album>().Select((a) => a).Where((a) => a.AlbumTitle.Equals(albumTitle) & a.AlbumArtist.Equals(albumArtist)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not get the Album with AlbumTitle='{0}' and AlbumArtist='{1}'. Exception: {2}", albumTitle, albumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    album = null;
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
            return album;
        }

        public override Album GetAlbum(long albumID)
        {
            Album album = null;

            try
            {
                using (var conn = this.Factory.GetConnection())
                {
                    try
                    {
                        album = conn.Table<Album>().Select((a) => a).Where((a) => a.AlbumID == albumID).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        LogClient.Current.Error("Could not get the Album with AlbumID='{0}'. Exception: {1}", albumID.ToString(), ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                album = null;
                LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
            }

            return album;
        }

        public override async Task<List<Album>> GetFrequentAlbumsAsync(int limit)
        {
            var albums = new List<Album>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            albums = conn.Query<Album>("SELECT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded, alb.DateCreated, MAX(ts.DateLastPlayed) AS maxdatelastplayed, SUM(ts.PlayCount) AS playcountsum FROM TrackStatistic ts " +
                                                       "INNER JOIN Track tra ON ts.SafePath = tra.SafePath " +
                                                       "INNER JOIN Album alb ON tra.AlbumID = alb.AlbumID " +
                                                       "WHERE ts.PlayCount IS NOT NULL AND ts.PlayCount > 0 " +
                                                       "GROUP BY alb.AlbumID " +
                                                       "ORDER BY playcountsum DESC, maxdatelastplayed DESC");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not get the Album history. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albums;
        }

        public override async Task<Album> AddAlbumAsync(Album album)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
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
                            LogClient.Current.Error("Could not create the Album with AlbumTitle='{0}' and AlbumArtist='{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return album;
        }

        public override async Task<bool> UpdateAlbumAsync(Album album)
        {
            bool isUpdateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            conn.Update(album);
                            isUpdateSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            LogClient.Current.Error("Could not update the Album with Title='{0}' and Album artist = '{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return isUpdateSuccess;
        }

        public override async Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID)
        {
            bool isUpdateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
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
                            LogClient.Current.Error("Could not update album artwork for album with title '{0}' and album artist '{1}'. Exception: {2}", albumTitle, albumArtist, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return isUpdateSuccess;
        }
        #endregion
    }
}
