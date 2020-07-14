using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private string SelectVisibleTracksQuery()
        {
            return @"SELECT DISTINCT t.TrackID, t.Artists, t.Genres, t.AlbumTitle, t.AlbumArtists, t.AlbumKey,
                     t.Path, t.SafePath, t.FileName, t.MimeType, t.FileSize, t.BitRate, 
                     t.SampleRate, t.TrackTitle, t.TrackNumber, t.TrackCount, t.DiscNumber,
                     t.DiscCount, t.Duration, t.Year, t.HasLyrics, t.DateAdded, t.DateFileCreated,
                     t.DateLastSynced, t.DateFileModified, t.NeedsIndexing, t.NeedsAlbumArtworkIndexing, t.IndexingSuccess,
                     t.IndexingFailureReason, t.Rating, t.Love, t.PlayCount, t.SkipCount, t.DateLastPlayed
                     FROM Track t
                     INNER JOIN FolderTrack ft ON ft.TrackID = t.TrackID
                     INNER JOIN Folder f ON ft.FolderID = f.FolderID
                     WHERE f.ShowInCollection = 1 AND t.IndexingSuccess = 1 AND t.NeedsIndexing = 0";
        }

        private string SelectedAlbumDataQueryPart()
        {
            return @"SELECT t.AlbumTitle, t.AlbumArtists, t.AlbumKey, 
                     MAX(t.TrackTitle) as TrackTitle,
                     MAX(t.Artists) as Artists,
                     MAX(t.Year) AS Year, 
                     MAX(t.DateFileCreated) AS DateFileCreated, 
                     MAX(t.DateAdded) AS DateAdded";
        }

        private string SelectAllAlbumDataQuery()
        {
            return $"{this.SelectedAlbumDataQueryPart()} FROM Track t";
        }

        private string SelectVisibleAlbumDataQuery()
        {
            return $@"{this.SelectAllAlbumDataQuery()}
                      INNER JOIN FolderTrack ft ON ft.TrackID = t.TrackID
                      INNER JOIN Folder f ON ft.FolderID = f.FolderID
                      WHERE f.ShowInCollection = 1 AND t.IndexingSuccess = 1";
        }

        public async Task<List<Track>> GetTracksAsync(IList<string> paths)
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            IList<string> safePaths = paths.Select((p) => p.ToSafePath()).ToList();

                            tracks = conn.Query<Track>($"{this.SelectVisibleTracksQuery()} AND {DataUtils.CreateInClause("t.SafePath", safePaths)};");
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

        public async Task<List<Track>> GetTracksAsync()
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            tracks = conn.Query<Track>($"{this.SelectVisibleTracksQuery()};");
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

        public async Task<List<Track>> GetTracksAsync(string whereClause)
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string query = $"{this.SelectVisibleTracksQuery()} AND {whereClause};";
                            tracks = conn.Query<Track>(query);
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

        public async Task<List<Track>> GetArtistTracksAsync(IList<string> artists)
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string query = $"{this.SelectVisibleTracksQuery()} AND ({DataUtils.CreateOrLikeClause("t.Artists", artists, Constants.ColumnValueDelimiter)} OR {DataUtils.CreateOrLikeClause("t.AlbumArtists", artists, Constants.ColumnValueDelimiter)});";

                            tracks = conn.Query<Track>(query);
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

        public async Task<List<Track>> GetGenreTracksAsync(IList<string> genreNames)
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            tracks = conn.Query<Track>($"{this.SelectVisibleTracksQuery()} AND {DataUtils.CreateOrLikeClause("t.Genres", genreNames, Constants.ColumnValueDelimiter)};");
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

        public async Task<List<Track>> GetAlbumTracksAsync(IList<string> albumKeys)
        {
            var tracks = new List<Track>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            tracks = conn.Query<Track>(this.SelectVisibleTracksQuery() + $" AND {DataUtils.CreateInClause("t.AlbumKey", albumKeys)};");
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

        public async Task<RemoveTracksResult> RemoveTracksAsync(IList<Track> tracks)
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
                            IList<string> pathsToRemove = tracks.Select((t) => t.Path).ToList();

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

        public async Task<IList<string>> GetGenresAsync()
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
                            genreNames = conn.Query<Track>(this.SelectVisibleTracksQuery()).ToList()
                                                           .Select((t) => t.Genres)
                                                           .SelectMany(g => DataUtils.SplitColumnMultiValue(g))
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

        public async Task<IList<string>> GetTrackArtistsAsync()
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
                            artistNames = conn.Query<Track>(this.SelectVisibleTracksQuery()).ToList()
                                                            .Select((t) => t.Artists)
                                                            .SelectMany(a => DataUtils.SplitColumnMultiValue(a))
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

        public async Task<IList<string>> GetAlbumArtistsAsync()
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
                            artistNames = conn.Query<Track>(this.SelectVisibleTracksQuery()).ToList()
                                                            .Select((t) => t.AlbumArtists)
                                                            .SelectMany(a => DataUtils.SplitColumnMultiValue(a))
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

            return artistNames;
        }

        public async Task<IList<AlbumData>> GetArtistAlbumDataAsync(IList<string> artists, ArtistType artistType)
        {
            var albumData = new List<AlbumData>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string filterQuery = string.Empty;

                            if (artists != null)
                            {
                                if (artistType.Equals(ArtistType.All))
                                {
                                    filterQuery = $" AND ({DataUtils.CreateOrLikeClause("Artists", artists, Constants.ColumnValueDelimiter)} OR {DataUtils.CreateOrLikeClause("AlbumArtists", artists, Constants.ColumnValueDelimiter)})";
                                }
                                else if (artistType.Equals(ArtistType.Track))
                                {
                                    filterQuery = $" AND ({DataUtils.CreateOrLikeClause("Artists", artists, Constants.ColumnValueDelimiter)})";
                                }
                                else if (artistType.Equals(ArtistType.Album))
                                {
                                    filterQuery = $" AND ({DataUtils.CreateOrLikeClause("AlbumArtists", artists, Constants.ColumnValueDelimiter)})";
                                }
                            }

                            string query = this.SelectVisibleAlbumDataQuery() + filterQuery + " GROUP BY AlbumKey";

                            albumData = conn.Query<AlbumData>(query);
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

            return albumData;
        }

        public async Task<IList<AlbumData>> GetGenreAlbumDataAsync(IList<string> genres)
        {
            var albumData = new List<AlbumData>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string filterQuery = string.Empty;

                            if (genres != null)
                            {
                                filterQuery = $" AND {DataUtils.CreateOrLikeClause("Genres", genres, Constants.ColumnValueDelimiter)}";
                            }

                            string query = this.SelectVisibleAlbumDataQuery() + filterQuery + " GROUP BY AlbumKey";

                            albumData = conn.Query<AlbumData>(query);
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

            return albumData;
        }

        public async Task<IList<AlbumData>> GetAllAlbumDataAsync()
        {
            var albumData = new List<AlbumData>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            string query = this.SelectVisibleAlbumDataQuery() + " GROUP BY AlbumKey";

                            albumData = conn.Query<AlbumData>(query);
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

            return albumData;
        }

        public async Task<IList<AlbumData>> GetAlbumDataToIndexAsync()
        {
            var albumDataToIndex = new List<AlbumData>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            albumDataToIndex = conn.Query<AlbumData>($@"{this.SelectAllAlbumDataQuery()}
                                                                        WHERE AlbumKey NOT IN (SELECT AlbumKey FROM AlbumArtwork) 
                                                                        AND AlbumKey IS NOT NULL AND AlbumKey <> ''
                                                                        AND NeedsAlbumArtworkIndexing=1 GROUP BY AlbumKey;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the albumKeys to index. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return albumDataToIndex;
        }

        public async Task<Track> GetLastModifiedTrackForAlbumKeyAsync(string albumKey)
        {
            Track lastModifiedTrack = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            lastModifiedTrack = conn.Table<Track>().Where((t) => t.AlbumKey.Equals(albumKey)).Select((t) => t).OrderByDescending((t) => t.DateFileModified).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the last modified track for the given albumKey. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return lastModifiedTrack;
        }

        public async Task DisableNeedsAlbumArtworkIndexingAsync(string albumKey)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"UPDATE Track SET NeedsAlbumArtworkIndexing=0 WHERE AlbumKey=?;", albumKey);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not disable NeedsAlbumArtworkIndexing for the given albumKey. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task DisableNeedsAlbumArtworkIndexingForAllTracksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute($"UPDATE Track SET NeedsAlbumArtworkIndexing=0;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not disable NeedsAlbumArtworkIndexing for all tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task EnableNeedsAlbumArtworkIndexingForAllTracksAsync(bool onlyWhenHasNoCover)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            if (onlyWhenHasNoCover)
                            {
                                conn.Execute($"UPDATE Track SET NeedsAlbumArtworkIndexing=1 WHERE AlbumKey NOT IN (SELECT AlbumKey FROM AlbumArtwork);");
                            }
                            else
                            {
                                conn.Execute($"UPDATE Track SET NeedsAlbumArtworkIndexing=1;");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error($"Could not disable NeedsAlbumArtworkIndexing for all tracks. {nameof(onlyWhenHasNoCover)}={onlyWhenHasNoCover}. Exception: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task UpdateRatingAsync(string path, int rating)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("UPDATE Track SET Rating=? WHERE SafePath=?", rating, path.ToSafePath());
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update rating for path='{0}'. Exception: {1}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }
        public async Task UpdateLoveAsync(string path, int love)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("UPDATE Track SET Love=? WHERE SafePath=?", love, path.ToSafePath());
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update love for path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task UpdatePlaybackCountersAsync(PlaybackCounter counters)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("UPDATE Track SET PlayCount=?, SkipCount=?, DateLastPlayed=? WHERE SafePath=?", counters.PlayCount, counters.SkipCount, counters.DateLastPlayed, counters.Path.ToSafePath());
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update statistics for path='{0}'. Exception: {1}", counters.Path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<PlaybackCounter> GetPlaybackCountersAsync(string path)
        {
            PlaybackCounter counters = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            counters = conn.Query<PlaybackCounter>("SELECT Path, SafePath, PlayCount, SkipCount, DateLastPlayed FROM Track WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get PlaybackCounters for path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return counters;
        }

        public Task<AlbumData> GetAlbumDataAsync(string albumKey)
        {
            AlbumData albumData = null;

            try
            {
                using (var conn = this.factory.GetConnection())
                {
                    try
                    {
                        albumData = conn.Query<AlbumData>($@"{this.SelectAllAlbumDataQuery()} WHERE AlbumKey=?;", albumKey).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not get AlbumData for albumKey='{0}'. Exception: {1}", albumKey, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
            }
            throw new NotImplementedException();
        }
    }
}
