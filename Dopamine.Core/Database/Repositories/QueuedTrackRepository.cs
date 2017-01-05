using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class QueuedTrackRepository : IQueuedTrackRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public QueuedTrackRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IQueuedTrackRepository
        public List<MergedTrack> GetSavedQueuedTracks()
        {
            var tracks = new List<MergedTrack>();

            try
            {
                using (var conn = this.factory.GetConnection())
                {
                    try
                    {
                        tracks = conn.Query<MergedTrack>("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path, tra.SafePath," +
                                                       " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                       " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                       " tra.Rating, tra.HasLyrics, tra.Love, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                       " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                       " alb.AlbumArtist, alb.Year AS AlbumYear" +
                                                       " FROM QueuedTrack qtra" +
                                                       " INNER JOIN Track tra ON qtra.SafePath=tra.SafePath" +
                                                       " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                       " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                       " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID" +
                                                       " ORDER BY qtra.OrderID");


                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not get Queued Tracks. Exception: {0}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
            }

            return tracks;
        }

        public async Task SaveQueuedTracksAsync(IList<string> paths, string playingPath, double progressSeconds)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // First, clear old queued tracks
                            conn.Execute("DELETE FROM QueuedTrack;");

                            // Then, insert new queued tracks (using a common transaction speeds up inserts and updates)
                            conn.Execute("BEGIN TRANSACTION;");

                            foreach (string path in paths)
                            {
                                conn.Execute("INSERT INTO QueuedTrack(OrderID,Path,SafePath) VALUES(0,?,?);", path, path.ToSafePath());
                            }

                            conn.Execute("UPDATE QueuedTrack SET OrderID=QueuedTrackID;");

                            try
                            {
                                if (playingPath != null)
                                {
                                    conn.Execute("UPDATE QueuedTrack SET IsPlaying=1, ProgressSeconds=? WHERE SafePath=?;", Convert.ToInt64(progressSeconds), playingPath.ToSafePath());
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("Could not update the playing queued track. Exception: {0}", ex.Message);
                            }

                            conn.Execute("COMMIT;");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not save queued tracks. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<QueuedTrack> GetPlayingTrackAsync()
        {
            QueuedTrack track = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            track = conn.Query<QueuedTrack>("SELECT * FROM QueuedTrack WHERE IsPlaying=1;").FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get the playing queued Track. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return track;
        }

        #endregion
    }
}
