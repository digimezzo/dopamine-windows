using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Digimezzo.Utilities.Log;

namespace Dopamine.Common.Database.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public ArtistRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IArtistRepository
        public async Task<List<Artist>> GetArtistsAsync()
        {
            var artists = new List<Artist>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            var albumArtists = new List<string>();

                            // Get the Track Artists
                            artists = conn.Query<Artist>("SELECT DISTINCT art.ArtistID, art.ArtistName FROM Artist art" +
                                                         " INNER JOIN Track tra ON art.ArtistID=tra.ArtistID" +
                                                         " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                         " WHERE fol.ShowInCollection=1");

                            // Get the Album Artists
                            albumArtists = conn.Query<Album>("SELECT DISTINCT alb.AlbumID, alb.AlbumTitle, alb.AlbumArtist, alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb" +
                                                           " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                           " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                           " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                           " WHERE tra.AlbumID=alb.AlbumID AND tra.ArtistID=tra.ArtistID AND fol.ShowInCollection=1").ToList().Select((a) => a.AlbumArtist).ToList();

                            foreach (string albumArtist in albumArtists)
                            {
                                if (!artists.Select((art) => art.ArtistName).Contains(albumArtist))
                                {
                                    artists.Add(new Artist { ArtistName = albumArtist });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Artists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return artists;
        }

        public async Task<Artist> GetArtistAsync(string artistName)
        {
            Artist artist = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            artist = conn.Table<Artist>().Select((a) => a).Where((a) => a.ArtistName.Equals(artistName)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Artist with ArtistName='{0}'. Exception: {1}", artistName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return artist;
        }

        public async Task<Artist> AddArtistAsync(Artist artist)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            long maxArtistID = conn.ExecuteScalar<long>("SELECT MAX(ArtistID) FROM Artist;");
                            artist.ArtistID = maxArtistID + 1;
                            conn.Insert(artist);
                        }
                        catch (Exception ex)
                        {
                            artist = null;
                            LogClient.Error("Could not create the Artist with ArtistName='{0}'. Exception: {1}", artist.ArtistName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return artist;
        }

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
        #endregion
    }
}
