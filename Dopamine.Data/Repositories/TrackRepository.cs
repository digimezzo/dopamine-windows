using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        private ISQLiteConnectionFactory factory;

        public TrackRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        private string DisplayableTracksQuery()
        {
            return @"FROM Track t
                     INNER JOIN FolderTrack ft ON ft.TrackID = t.TrackID
                     INNER JOIN Folder f ON ft.FolderID = f.FolderID
                     WHERE f.ShowInCollection = 1 AND t.IndexingSuccess = 1";
        }

        private string ArtistsFilterQuery(IList<string> artists)
        {
            var sb = new StringBuilder();

            sb.AppendLine(" AND (");

            var orClauses = new List<string>();

            foreach (string artist in artists)
            {
                if (string.IsNullOrEmpty(artist))
                {
                    orClauses.Add($@"Artists IS NULL OR Artists='' OR AlbumArtists IS NULL OR AlbumArtists=''");
                }
                else
                {
                    orClauses.Add($@"LOWER(Artists) LIKE '%{artist.Replace("'", "''").ToLower()}%' OR LOWER(AlbumArtists) LIKE '%{artist.Replace("'", "''").ToLower()}%'");
                }
            }

            sb.AppendLine(string.Join(" OR ", orClauses.ToArray()));
            sb.AppendLine(")");

            return sb.ToString();
        }

        private string GenresFilterQuery(IList<string> genres)
        {
            var sb = new StringBuilder();

            sb.AppendLine(" AND (");

            var orClauses = new List<string>();

            foreach (string genre in genres)
            {
                if (string.IsNullOrEmpty(genre))
                {
                    orClauses.Add($@"Genres IS NULL OR Genres=''");
                }
                else
                {
                    orClauses.Add($@"LOWER(Genres) LIKE '%{genre.Replace("'", "''").ToLower()}%'");
                }
            }

            sb.AppendLine(string.Join(" OR ", orClauses.ToArray()));
            sb.AppendLine(")");

            return sb.ToString();
        }

        private string SelectQueryPart()
        {
            return "SELECT DISTINCT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.Path, tra.SafePath, " +
                   "tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle, " +
                   "tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year, " +
                   "tra.HasLyrics, tra.DateAdded, tra.DateLastSynced, " +
                   "tra.DateFileModified, tra.MetaDataHash, " +
                   "alb.Year AS AlbumYear, " +
                   "ts.Rating, ts.Love, ts.PlayCount, ts.SkipCount, ts.DateLastPlayed " +
                   "FROM Track tra " +
                   "INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID " +
                   "INNER JOIN Artist art ON tra.ArtistID=art.ArtistID " +
                   "INNER JOIN Genre gen ON tra.GenreID=gen.GenreID " +
                   "INNER JOIN TrackStatistic ts ON tra.SafePath=ts.SafePath ";
        }

        public async Task<List<PlayableTrack>> GetTracksAsync(IList<string> paths)
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            //var safePaths = paths.Select((p) => p.ToSafePath()).ToList();

                            //string q = string.Format(this.SelectQueryPart() +
                            //                         "WHERE tra.SafePath IN ({0});", DatabaseUtils.ToQueryList(safePaths));

                            //tracks = conn.Query<PlayableTrack>(q);
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

        public async Task<List<PlayableTrack>> GetTracksAsync()
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            //tracks = conn.Query<PlayableTrack>(this.SelectQueryPart() +
                            //                                 "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                            //                                 "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID " +
                            //                                 "WHERE fol.ShowInCollection=1 AND tra.IndexingSuccess=1;");
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

        public async Task<List<PlayableTrack>> GetArtistTracksAsync(IList<Artist> artists)
        {
            var tracks = new List<PlayableTrack>();

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
                                                     "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                     "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID " +
                                                     "WHERE (tra.ArtistID IN ({0}) OR alb.AlbumArtist IN ({1})) AND fol.ShowInCollection=1 AND tra.IndexingSuccess=1;", DatabaseUtils.ToQueryList(artistIDs), DatabaseUtils.ToQueryList(artistNames));

                            tracks = conn.Query<PlayableTrack>(q);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the Tracks for Artists. Exception: {0}", ex.Message);
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

        public async Task<List<PlayableTrack>> GetGenreTracksAsync(IList<long> genreIds)
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string q = string.Format(this.SelectQueryPart() +
                                                     "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                     "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID " +
                                                     "WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1 AND tra.IndexingSuccess=1;", DatabaseUtils.ToQueryList(genreIds));

                            tracks = conn.Query<PlayableTrack>(q);
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

        public async Task<List<PlayableTrack>> GetAlbumTracksAsync(IList<long> albumIds)
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string q = string.Format(this.SelectQueryPart() +
                                                     "INNER JOIN FolderTrack ft ON ft.TrackID=tra.TrackID " +
                                                     "INNER JOIN Folder fol ON ft.FolderID=fol.FolderID " +
                                                     "WHERE tra.AlbumID IN ({0}) AND fol.ShowInCollection=1 AND tra.IndexingSuccess=1;", DatabaseUtils.ToQueryList(albumIds));

                            tracks = conn.Query<PlayableTrack>(q);
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

        public async Task<RemoveTracksResult> RemoveTracksAsync(IList<PlayableTrack> tracks)
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

        public async Task ClearRemovedTrackAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    try
                    {
                        using (var conn = this.factory.GetConnection())
                        {
                            conn.Execute("DELETE FROM RemovedTrack;");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not clear removed tracks. Exception: {0}", ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
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

        public async Task<IList<string>> GetAllGenresAsync()
        {
            var genreNames = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            genreNames = conn.Query<Track>("SELECT * " + this.DisplayableTracksQuery()).ToList()
                                                            .Select((t) => t.Genres).Where(g => !string.IsNullOrEmpty(g))
                                                            .SelectMany(g => g.Split(Convert.ToChar(Constants.MultiValueTagsSeparator)))
                                                            .Distinct().ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the genres. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return genreNames;
        }

        public async Task<IList<string>> GetAllTrackArtistsAsync()
        {
            var artistNames = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            artistNames = conn.Query<Track>("SELECT * " + this.DisplayableTracksQuery()).ToList()
                                                            .Select((t) => t.Artists).Where(a => !string.IsNullOrEmpty(a))
                                                            .SelectMany(a => a.Split(Convert.ToChar(Constants.MultiValueTagsSeparator)))
                                                            .Distinct().ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the track artists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return artistNames;
        }

        public async Task<IList<string>> GetAllAlbumArtistsAsync()
        {
            var albumArtists = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            albumArtists = conn.Query<Track>("SELECT * " + this.DisplayableTracksQuery()).ToList()
                                                            .Select((t) => t.AlbumArtists).Where(a => !string.IsNullOrEmpty(a))
                                                            .SelectMany(a => a.Split(Convert.ToChar(Constants.MultiValueTagsSeparator)))
                                                            .Distinct().ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the album artists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albumArtists;
        }

        public async Task<IList<AlbumData>> GetAlbumsAsync(IList<string> artists, IList<string> genres)
        {
            var albumValues = new List<AlbumData>();

            await Task.Run(() =>
                {
                    try
                    {
                        using (var conn = this.factory.GetConnection())
                        {
                            try
                            {
                                var filterQuery = string.Empty;

                                if (artists != null)
                                {
                                    filterQuery = this.ArtistsFilterQuery(artists);
                                }
                                else if (genres != null)
                                {
                                    filterQuery = this.GenresFilterQuery(genres);
                                }

                                albumValues = conn.Query<AlbumData>(@"SELECT AlbumTitle, AlbumArtists, AlbumKey, 
                                                                  MAX(Year) AS Year, MAX(DateFileCreated) AS DateFileCreated, 
                                                                  MAX(DateAdded) AS DateAdded " +
                                                                  this.DisplayableTracksQuery() +
                                                                  filterQuery +
                                                                  " GROUP BY AlbumKey");
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("Could not get all the album values. Exception: {0}", ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                    }
                });

            return albumValues;
        }
    }
}
