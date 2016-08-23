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
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public TrackRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region ITrackRepository
        public async Task<List<TrackInfo>> GetTracksAsync(IList<string> paths)
        {
            var tracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string q = string.Format("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                     " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                     " FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                     " WHERE tra.Path IN ({0});", Utils.ToQueryList(paths));

                            tracks = conn.Query<TrackInfo>(q);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            tracks = conn.Query<TrackInfo>("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                           " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                           " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                           " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                           " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                           " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                           " FROM Track tra" +
                                                           " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                           " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                           " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                           " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                           " WHERE fol.ShowInCollection=1;");
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> artistIDs = artists.Select((a) => a.ArtistID).ToList();
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            string q = string.Format("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                     " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                     " FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE (tra.ArtistID IN ({0}) OR alb.AlbumArtist IN ({1})) AND fol.ShowInCollection=1;", Utils.ToQueryList(artistIDs), Utils.ToQueryList(artistNames));

                            tracks = conn.Query<TrackInfo>(q);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            string q = string.Format("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                     " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                     " FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1;", Utils.ToQueryList(genreIDs));

                            tracks = conn.Query<TrackInfo>(q);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> albumIDs = albums.Select((a) => a.AlbumID).ToList();

                            string q = string.Format("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                     " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                     " FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.AlbumID IN ({0}) AND fol.ShowInCollection=1;", Utils.ToQueryList(albumIDs));

                            tracks = conn.Query<TrackInfo>(q);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            var playlistIDs = new List<long>();

                            if (playlists != null) playlistIDs = playlists.Select((p) => p.PlaylistID).ToList();

                            List<long> trackIDs = conn.Table<PlaylistEntry>().Where((t) => playlistIDs.Contains(t.PlaylistID)).Select((t) => t.TrackID).ToList();

                            string q = string.Format("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                     " alb.AlbumArtist, alb.Year AS AlbumYear, alb.ArtworkID AS AlbumArtworkID" +
                                                     " FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                     " WHERE tra.TrackID IN ({0});", Utils.ToQueryList(trackIDs));

                            tracks = conn.Query<TrackInfo>(q);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            track = conn.Table<Track>().Select((t) => t).Where((t) => t.Path.Equals(path)).FirstOrDefault();
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
                    using (var conn = this.factory.GetConnection())
                    {
                        List<string> pathsToRemove = tracks.Select((t) => t.Path).ToList();
                        List<string> removedPaths = conn.Table<RemovedTrack>().Select((t) => t.Path).ToList();
                        List<string> pathsToRemoveAndRecord = pathsToRemove.Except(removedPaths).ToList();

                        // Add the Track to the table of Removed Tracks (only if it is not already in there)
                        foreach (string path in pathsToRemoveAndRecord)
                        {
                            try
                            {
                                conn.Insert(new RemovedTrack { DateRemoved = DateTime.Now.Ticks, Path = path });
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not add the track with path '{0}' to the table of removed tracks. Exception: {1}", path, ex.Message);
                                result = RemoveTracksResult.Error;
                            }
                        }

                        List<Track> tracksToRemove = conn.Table<Track>().Select((t) => t).Where((t) => pathsToRemove.Contains(t.Path)).ToList();
                        conn.Delete(tracksToRemove);
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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Update(track);

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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            Track dbTrack = conn.Table<Track>().Select((t) => t).Where((t) => t.Path.Equals(path)).FirstOrDefault();

                            if (dbTrack != null)
                            {
                                dbTrack.FileSize = FileOperations.GetFileSize(path);
                                dbTrack.DateFileModified = FileOperations.GetDateModified(path);
                                dbTrack.DateLastSynced = DateTime.Now.Ticks;

                                conn.Update(dbTrack);

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
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            foreach (TrackStatistic stat in trackStatistics)
                            {
                                Track dbTrack = conn.Table<Track>().Select((t) => t).Where((t) => t.Path == stat.Path).FirstOrDefault();

                                if (dbTrack != null)
                                {
                                    if (dbTrack.PlayCount == null) dbTrack.PlayCount = 0;
                                    dbTrack.PlayCount += stat.PlayCount;
                                    dbTrack.DateLastPlayed = stat.DateLastPlayed;

                                    if (dbTrack.SkipCount == null) dbTrack.SkipCount = 0;
                                    dbTrack.SkipCount += stat.SkipCount;

                                    conn.Update(dbTrack);
                                }
                            }
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
