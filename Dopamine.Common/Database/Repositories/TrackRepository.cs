using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Extensions;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories
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

        #region Private
        private string SelectQueryPart()
        {
            return "SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path, tra.SafePath, " +
                   "tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle, " +
                   "tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year, " +
                   "tra.Rating, tra.HasLyrics, tra.Love, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced, " +
                   "tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle, " +
                   "alb.AlbumArtist, alb.Year AS AlbumYear " +
                   "FROM Track tra " +
                   "INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID " +
                   "INNER JOIN Artist art ON tra.ArtistID=art.ArtistID " +
                   "INNER JOIN Genre gen ON tra.GenreID=gen.GenreID ";
        }
        #endregion

        #region ITrackRepository
        public async Task<List<MergedTrack>> GetTracksAsync(IList<string> paths)
        {
            var tracks = new List<MergedTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            var safePaths = paths.Select((p) => p.ToSafePath()).ToList();

                            string q = string.Format(this.SelectQueryPart() + 
                                                     "WHERE tra.SafePath IN ({0});", Utils.ToQueryList(safePaths));

                            tracks = conn.Query<MergedTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Paths. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<MergedTrack>> GetTracksAsync()
        {
            var tracks = new List<MergedTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            tracks = conn.Query<MergedTrack>(this.SelectQueryPart() +
                                                             "INNER JOIN Folder fol ON tra.FolderID=fol.FolderID " +
                                                             "WHERE fol.ShowInCollection=1;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the Tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<MergedTrack>> GetTracksAsync(IList<Artist> artists)
        {
            var tracks = new List<MergedTrack>();

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

                            string q = string.Format(this.SelectQueryPart() + 
                                                     "INNER JOIN Folder fol ON tra.FolderID=fol.FolderID " +
                                                     "WHERE (tra.ArtistID IN ({0}) OR alb.AlbumArtist IN ({1})) AND fol.ShowInCollection=1;", Utils.ToQueryList(artistIDs), Utils.ToQueryList(artistNames));

                            tracks = conn.Query<MergedTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<MergedTrack>> GetTracksAsync(IList<Genre> genres)
        {
            var tracks = new List<MergedTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            string q = string.Format(this.SelectQueryPart() +
                                                     "INNER JOIN Folder fol ON tra.FolderID=fol.FolderID " +
                                                     "WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1;", Utils.ToQueryList(genreIDs));

                            tracks = conn.Query<MergedTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<MergedTrack>> GetTracksAsync(IList<Album> albums)
        {
            var tracks = new List<MergedTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            List<long> albumIDs = albums.Select((a) => a.AlbumID).ToList();

                            string q = string.Format(this.SelectQueryPart() +
                                                     "INNER JOIN Folder fol ON tra.FolderID=fol.FolderID " +
                                                     "WHERE tra.AlbumID IN ({0}) AND fol.ShowInCollection=1;", Utils.ToQueryList(albumIDs));

                            tracks = conn.Query<MergedTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Albums. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public async Task<List<MergedTrack>> GetTracksAsync(IList<Playlist> playlists)
        {
            var tracks = new List<MergedTrack>();

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

                            string q = string.Format(this.SelectQueryPart() +
                                                     "INNER JOIN PlaylistEntry ple ON tra.TrackID=ple.TrackID " +
                                                     "WHERE ple.PlaylistID IN ({0}) " +
                                                     "ORDER BY ple.EntryID;", Utils.ToQueryList(playlistIDs));

                            tracks = conn.Query<MergedTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Playlists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return tracks;
        }

        public Track GetTrack(string path)
        {
            Track track = null;

            try
            {
                using (var conn = this.factory.GetConnection())
                {
                    try
                    {
                        track = conn.Query<Track>("SELECT * FROM Track WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not get the Track with Path='{0}'. Exception: {1}", path, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
            }

            return track;
        }

        public async Task<Track> GetTrackAsync(string path)
        {
            Track track = null;

            await Task.Run(() =>
            {
                track = this.GetTrack(path);
            });

            return track;
        }

        public async Task<RemoveTracksResult> RemoveTracksAsync(IList<MergedTrack> tracks)
        {
            RemoveTracksResult result = RemoveTracksResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    try
                    {
                        using (var conn = this.factory.GetConnection())
                        {
                            List<string> pathsToRemove = tracks.Select((t) => t.Path).ToList();

                            conn.Execute("BEGIN TRANSACTION");

                            foreach (string path in pathsToRemove)
                            {
                                // Add to table RemovedTrack, only if not already present.
                                conn.Execute("INSERT INTO RemovedTrack(DateRemoved, Path, SafePath) SELECT ?,?,? WHERE NOT EXISTS (SELECT 1 FROM RemovedTrack WHERE SafePath=?)", DateTime.Now.Ticks, path, path.ToSafePath(), path.ToSafePath());

                                // Remove from QueuedTrack
                                conn.Execute("DELETE FROM QueuedTrack WHERE SafePath=?", path.ToSafePath());

                                // Remove from Track
                                conn.Execute("DELETE FROM Track WHERE SafePath=?", path.ToSafePath());
                            }

                            conn.Execute("COMMIT");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could remove tracks from the database. Exception: {0}", ex.Message);
                        result = RemoveTracksResult.Error;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
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
                            LogClient.Error("Could not update the Track with path='{0}'. Exception: {1}", track.Path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
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
                            Track dbTrack = conn.Query<Track>("SELECT * FROM Track WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();

                            if (dbTrack != null)
                            {
                                dbTrack.FileSize = FileUtils.SizeInBytes(path);
                                dbTrack.DateFileModified = FileUtils.DateModifiedTicks(path);
                                dbTrack.DateLastSynced = DateTime.Now.Ticks;

                                conn.Update(dbTrack);

                                updateSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update file information for Track with Path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
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
                                Track dbTrack = conn.Query<Track>("SELECT * FROM Track WHERE SafePath=?", stat.Path.ToSafePath()).FirstOrDefault();

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
                            LogClient.Error("Could not save track statistics. Exception: {0}", ex.Message);
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
