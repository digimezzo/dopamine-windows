using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Dopamine.Core.IO;

namespace Dopamine.Core.Database.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        #region ITrackRepository
        public async Task<List<TrackInfo>> GetTracksAsync(IList<string> paths)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      where paths.Contains(tra.Path)
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Tracks for Paths. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<TrackInfo>> GetTracksAsync()
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      join fol in db.Folders on tra.FolderID equals fol.FolderID
                                      where fol.ShowInCollection == 1
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get all the Tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<TrackInfo>> GetTracksAsync(IList<Artist> artists)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Extracting these lists is not supported in the Linq to SQL query.
                            // That is why it is done outside the Linq to SQL query.
                            List<long> artistIDs = artists.Select((a) => a.ArtistID).ToList();
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      join fol in db.Folders on tra.FolderID equals fol.FolderID
                                      where (artistIDs.Contains(tra.ArtistID) | artistNames.Contains(alb.AlbumArtist)) & fol.ShowInCollection == 1
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Tracks for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<TrackInfo>> GetTracksAsync(IList<Genre> genres)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Extracting this list is not supported in the Linq to SQL query.
                            // That is why it is done outside the Linq to SQL query.
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      join fol in db.Folders on tra.FolderID equals fol.FolderID
                                      where genreIDs.Contains(tra.GenreID) & fol.ShowInCollection == 1
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Tracks for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<TrackInfo>> GetTracksAsync(IList<Album> albums)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Extracting this list is not supported in the Linq to SQL query.
                            // That is why it is done outside the Linq to SQL query.
                            List<long> albumIDs = albums.Select((a) => a.AlbumID).ToList();

                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      join fol in db.Folders on tra.FolderID equals fol.FolderID
                                      where albumIDs.Contains(tra.AlbumID) & fol.ShowInCollection == 1
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Tracks for Albums. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<TrackInfo>> GetTracksAsync(IList<Playlist> playlists)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            var playlistIds = new List<long>();

                            if (playlists != null)
                            {
                                playlistIds = playlists.Select((pl) => pl.PlaylistID).ToList();
                            }

                            List<long> trackIds = db.PlaylistEntries.Where((t) => playlistIds.Contains(t.PlaylistID)).Select((t) => t.TrackID).ToList();

                            tracks = (from tra in db.Tracks
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
                                      where trackIds.Contains(tra.TrackID)
                                      select new TrackInfo
                                      {
                                          Track = tra,
                                          Artist = art,
                                          Genre = gen,
                                          Album = alb
                                      }).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Tracks for Playlists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<Track> GetTrackAsync(string path)
        {
            Track track = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            track = db.Tracks.Select((t) => t).Where((t) => t.Path.Equals(path)).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Track with Path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return track;
        }

        public async Task<RemoveTracksResult> RemoveTracksAsync(IList<TrackInfo> tracks)
        {
            RemoveTracksResult result = RemoveTracksResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        List<string> pathsToRemove = tracks.Select((t) => t.Track.Path).ToList();
                        List<string> removedPaths = db.RemovedTracks.Select((t) => t.Path).ToList();
                        List<string> pathsToRemoveAndRecord = pathsToRemove.Except(removedPaths).ToList();

                        // Add the Track to the table of Removed Tracks (only if it is not already in there)
                        foreach (string path in pathsToRemoveAndRecord)
                        {
                            try
                            {
                                db.RemovedTracks.Add(new RemovedTrack { DateRemoved = DateTime.Now.Ticks, Path = path });
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not add the track with path '{0}' to the table of removed tracks. Exception: {1}", path, ex.Message);
                                result = RemoveTracksResult.Error;
                            }
                        }

                        List<Track> tracksToRemove = db.Tracks.Select((t) => t).Where((t) => pathsToRemove.Contains(t.Path)).ToList();
                        db.Tracks.RemoveRange(tracksToRemove);

                        // Save the changes to the database
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                            result = RemoveTracksResult.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                    result = RemoveTracksResult.Error;
                }
            });

            return result;
        }
        public async Task<bool> UpdateTrackAsync(Track track)
        {
            bool isUpdateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.Entry(track).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            isUpdateSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not update the Track with path='{0}'. Exception: {1}", track.Path, ex.Message);
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
        public async Task<bool> UpdateTrackFileInformationAsync(string path)
        {
            bool updateSuccess = false;

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            Track dbTrack = db.Tracks.Select((t) => t).Where((t) => t.Path.Equals(path)).FirstOrDefault();

                            if (dbTrack != null)
                            {
                                dbTrack.FileSize = FileOperations.GetFileSize(path);
                                dbTrack.DateFileModified = FileOperations.GetDateModified(path);
                                dbTrack.DateLastSynced = DateTime.Now.Ticks;

                                db.SaveChanges();

                                updateSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not update file information for Track with Path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return updateSuccess;
        }
        public async Task SaveTrackStatisticsAsync(IList<TrackStatistic> trackStatistics)
        {
            if (trackStatistics == null) return;

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            foreach (TrackStatistic stat in trackStatistics)
                            {
                                Track dbTrack = db.Tracks.Select((t) => t).Where((t) => t.Path == stat.Path).FirstOrDefault();

                                if (dbTrack != null)
                                {
                                    if (dbTrack.PlayCount == null) dbTrack.PlayCount = 0;
                                    dbTrack.PlayCount += stat.PlayCount;
                                    dbTrack.DateLastPlayed = stat.DateLastPlayed;

                                    if (dbTrack.SkipCount == null) dbTrack.SkipCount = 0;
                                    dbTrack.SkipCount += stat.SkipCount;
                                }

                            }

                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not save track statistics. Exception: {0}", ex.Message);
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
