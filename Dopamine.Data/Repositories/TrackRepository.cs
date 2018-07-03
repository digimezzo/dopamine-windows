using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
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

        private string SelectTracksQuery()
        {
            return @"SELECT DISTINCT t.TrackID, t.Artists, t.Genres, t.AlbumTitle, t.AlbumArtists, t.AlbumKey,
                     t.Path, t.SafePath, t.FileName, t.MimeType, t.FileSize, t.BitRate, 
                     t.SampleRate, t.TrackTitle, t.TrackNumber, t.TrackCount, t.DiscNumber,
                     t.DiscCount, t.Duration, t.Year, t.HasLyrics, t.DateAdded, t.DateFileCreated,
                     t.DateLastSynced, t.DateFileModified, t.NeedsIndexing, t.IndexingSuccess,
                     t.IndexingFailureReason, t.Rating, t.Love, t.PlayCount, t.SkipCount, t.DateLastPlayed
                     FROM Track t
                     INNER JOIN FolderTrack ft ON ft.TrackID = t.TrackID
                     INNER JOIN Folder f ON ft.FolderID = f.FolderID
                     WHERE f.ShowInCollection = 1 AND t.IndexingSuccess = 1";
        }

        private string SelectAlbumDataQuery()
        {
            return @"SELECT AlbumTitle, AlbumArtists, AlbumKey, 
                     MAX(Year) AS Year, MAX(DateFileCreated) AS DateFileCreated, 
                     MAX(DateAdded) AS DateAdded
                     FROM Track t
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

                            tracks = conn.Query<Track>($"{this.SelectTracksQuery()} AND {DataUtils.CreateInClause("t.SafePath", safePaths)};");
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
                            tracks = conn.Query<Track>($"{this.SelectTracksQuery()};");
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

        public async Task<List<Track>> GetArtistTracksAsync(IList<string> artistNames)
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
                            tracks = conn.Query<Track>($"{this.SelectTracksQuery()} AND {DataUtils.CreateInClause("t.Artists", artistNames)} OR {DataUtils.CreateInClause("t.AlbumArtists", artistNames)};");
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
                            tracks = conn.Query<Track>($"{this.SelectTracksQuery()} AND {DataUtils.CreateInClause("t.Genres", genreNames)};");
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
                            tracks = conn.Query<Track>(this.SelectTracksQuery() + $" AND {DataUtils.CreateInClause("t.AlbumKey", albumKeys)};");
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
                            genreNames = conn.Query<Track>(this.SelectTracksQuery()).ToList()
                                                           .Select((t) => t.Genres).Where(g => !string.IsNullOrEmpty(g))
                                                           .SelectMany(g => MetadataUtils.GetMultiValueTagsCollection(g))
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
                            artistNames = conn.Query<Track>(this.SelectTracksQuery()).ToList()
                                                            .Select((t) => t.Artists).Where(a => !string.IsNullOrEmpty(a))
                                                            .SelectMany(a => MetadataUtils.GetMultiValueTagsCollection(a))
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
            var albumArtists = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            albumArtists = conn.Query<Track>(this.SelectTracksQuery()).ToList()
                                                             .Select((t) => t.AlbumArtists).Where(a => !string.IsNullOrEmpty(a))
                                                             .SelectMany(a => MetadataUtils.GetMultiValueTagsCollection(a))
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
                                string filterQuery = string.Empty;

                                if (artists != null)
                                {
                                    filterQuery = $" AND ({DataUtils.CreateOrLikeClause("Artists", artists)} OR {DataUtils.CreateOrLikeClause("AlbumArtists", artists)})";
                                }
                                else if (genres != null)
                                {
                                    filterQuery = $" AND {DataUtils.CreateOrLikeClause("Genres", genres)}";
                                }

                                albumValues = conn.Query<AlbumData>(this.SelectAlbumDataQuery() + filterQuery + " GROUP BY AlbumKey");
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
