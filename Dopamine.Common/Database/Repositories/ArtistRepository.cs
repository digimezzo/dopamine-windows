using Dopamine.Common.Base;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private ISQLiteConnectionFactory factory;
        private ILocalizationInfo info;

        public ISQLiteConnectionFactory Factory => this.factory;

        public ArtistRepository(ISQLiteConnectionFactory factory, ILocalizationInfo info)
        {
            this.factory = factory;
            this.info = info;
        }

        public async Task<List<Artist>> GetArtistsAsync(ArtistOrder artistOrder)
        {
            var artists = new List<Artist>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.Factory.GetConnection())
                    {
                        try
                        {
                            var trackArtists = new List<Artist>();
                            var albumArtists = new List<string>();

                            // Get the Track Artists
                            trackArtists = conn.Query<Artist>("SELECT DISTINCT art.ArtistID, " +
                                                              $"REPLACE(art.ArtistName,'{Defaults.UnknownArtistText}','{this.info.UnknownArtistText}') ArtistName " +
                                                              "FROM Artist art " +
                                                              "INNER JOIN Track tra ON art.ArtistID=tra.ArtistID " +
                                                              "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                              "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID AND ft.TrackID=tra.TrackID " +
                                                              "WHERE fol.ShowInCollection=1");

                            // Get the Album Artists
                            var albums = conn.Query<Album>("SELECT DISTINCT alb.AlbumID, " +
                                                           $"REPLACE(alb.AlbumTitle,'{Defaults.UnknownAlbumText}','{this.info.UnknownAlbumText}') AlbumTitle, " +
                                                           $"REPLACE(alb.AlbumArtist,'{Defaults.UnknownArtistText}','{this.info.UnknownArtistText}') AlbumArtist, " +
                                                           "alb.Year, alb.ArtworkID, alb.DateLastSynced, alb.DateAdded FROM Album alb " +
                                                           "INNER JOIN Track tra ON alb.AlbumID=tra.AlbumID " +
                                                           "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                           "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID AND ft.TrackID=tra.TrackID " +
                                                           "INNER JOIN Artist art ON tra.ArtistID=art.ArtistID " +
                                                           "WHERE tra.AlbumID=alb.AlbumID AND tra.ArtistID=tra.ArtistID AND fol.ShowInCollection=1");

                            albumArtists = albums.Select((a) => a.AlbumArtist).ToList();

                            if (artistOrder == ArtistOrder.All | artistOrder == ArtistOrder.Track)
                            {
                                foreach (Artist trackArtist in trackArtists)
                                {
                                    artists.Add(trackArtist);
                                }
                            }

                            if (artistOrder == ArtistOrder.All | artistOrder == ArtistOrder.Album)
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
                            artists = artists.OrderBy((a) => DatabaseUtils.GetSortableString(a.ArtistName, true)).ToList();
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
                    using (var conn = this.Factory.GetConnection())
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
                    using (var conn = this.Factory.GetConnection())
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
    }
}
