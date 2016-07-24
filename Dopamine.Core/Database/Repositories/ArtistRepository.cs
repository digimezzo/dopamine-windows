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
        #region IArtistRepository
        public async Task<List<Artist>> GetArtistsAsync(ArtistType artistType)
        {
            var artists = new List<Artist>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            var trackArtists = new List<Artist>();
                            var albumArtists = new List<string>();

                            // Get the Track Artists
                            trackArtists = (from art in db.Artists
                                            join tra in db.Tracks on art.ArtistID equals tra.ArtistID
                                            join fol in db.Folders on tra.FolderID equals fol.FolderID
                                            where fol.ShowInCollection == 1
                                            select art).Distinct().ToList();

                            // Get the Album Artists
                            albumArtists = (from alb in db.Albums
                                            join tra in db.Tracks on alb.AlbumID equals tra.AlbumID
                                            join fol in db.Folders on tra.FolderID equals fol.FolderID
                                            join art in db.Artists on tra.ArtistID equals art.ArtistID
                                            where tra.AlbumID == alb.AlbumID & tra.ArtistID == tra.ArtistID & fol.ShowInCollection == 1
                                            select alb.AlbumArtist).Distinct().ToList();

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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            artist = db.Artists.Select((a) => a).Where((a) => a.ArtistName.Equals(artistName)).FirstOrDefault();
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.Artists.Add(artist);
                            db.SaveChanges();
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.Artists.RemoveRange(db.Artists.Where((a) => !db.Tracks.Select((t) => t.ArtistID).Distinct().Contains(a.ArtistID)));
                            db.SaveChanges();
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
