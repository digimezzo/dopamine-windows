using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dopamine.Core.Logging;

namespace Dopamine.Core.Database.Repositories
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
        public async Task<List<Artist>> GetArtistsAsync(ArtistType artistType)
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
                            var trackArtists = new List<Artist>();
                            var albumArtists = new List<string>();

                            // Get the Track Artists
                            trackArtists = conn.Query<Artist>("SELECT DISTINCT * FROM Artist art" +
                                                              " INNER JOIN Track tra ON art.ArtistID=tra.ArtistID" +
                                                              " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                              " WHERE fol.ShowInCollection=1");

                            // Get the Album Artists
                            var albums = conn.Query<Album>("SELECT DISTINCT * FROM Album alb" +
                                                           " INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID" +
                                                           " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                           " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                           " WHERE tra.AlbumID=alb.AlbumID AND tra.ArtistID=tra.ArtistID AND fol.ShowInCollection=1");

                            albumArtists = albums.Select((a) => a.AlbumArtist).ToList();

                            if (artistType == ArtistType.All | artistType == ArtistType.Track)
                            {
                                foreach (Artist trackArtist in trackArtists)
                                {
                                    artists.Add(trackArtist);
                                }
                            }

                            if (artistType == ArtistType.All | artistType == ArtistType.Album)
                            {
                                foreach (string albumArtist in albumArtists)
                                {
                                    if (!artists.Select((art) => art.ArtistName).Contains(albumArtist))
                                    {
                                        artists.Add(new Artist { ArtistName = albumArtist });
                                    }
                                }
                            }

                            // Orders the artists
                            artists = artists.OrderBy((a) => Utils.GetSortableString(a.ArtistName, true)).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Artists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            LogClient.Instance.Logger.Error("Could not get the Artist with ArtistName='{0}'. Exception: {1}", artistName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            conn.Insert(artist);
                        }
                        catch (Exception ex)
                        {
                            artist = null;
                            LogClient.Instance.Logger.Error("Could not create the Artist with ArtistName='{0}'. Exception: {1}", artist.ArtistName, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                            LogClient.Instance.Logger.Error("There was a problem while deleting orphaned Artists. Exception: {0}", ex.Message);
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
