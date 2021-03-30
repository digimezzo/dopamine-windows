using Digimezzo.Foundation.Core.Logging;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class QueuedTrackRepository : IQueuedTrackRepository
    {
        private ISQLiteConnectionFactory factory;

        public QueuedTrackRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public async Task<List<Track>> GetSavedQueuedTracksAsync()
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
                            tracks = conn.Query<Track>(@"
                                SELECT * FROM Track t
                                JOIN QueuedTrack qt ON t.TrackID = qt.TrackID
                                ORDER BY qt.OrderID");
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get Queued tracks. Exception: {0}", ex.Message);
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

        public async Task SaveQueuedTracksAsync(IList<Track> tracks, long? currentTrackId, long progressSeconds)
        {                                                            
            await Task.Run(() =>
            {
                try
                {
                    var queuedTracks = new List<QueuedTrack>();

                    for (int i = 0; i < tracks.Count; ++i)
                    {
                        var track = tracks[i];

                        queuedTracks.Add(new QueuedTrack
                        {
                            TrackID = track.TrackID,
                            IsPlaying = track.TrackID == currentTrackId ? 1 : 0,
                            ProgressSeconds = track.TrackID == currentTrackId ? progressSeconds : 0,
                            OrderID = i
                        });
                    }

                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.BeginTransaction();
                            conn.Execute("DELETE FROM QueuedTrack;"); // First, clear old queued tracks.
                            conn.InsertAll(queuedTracks); // Then, insert new queued tracks.
                            conn.Commit();
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

        public async Task<Tuple<Track, long>> GetPlayingTrackAsync()
        {
            Tuple<Track, long> result = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            var track = conn.Query<Track>(@"
                                SELECT * FROM Track t
                                JOIN QueuedTrack qt
                                WHERE qt.IsPlaying = 1").FirstOrDefault();

                            if(track != null)
                            {
                                var progressSeconds = conn.ExecuteScalar<long>("SELECT ProgressSeconds FROM QueuedTrack WHERE TrackID = ?", track.TrackID);
                                result = Tuple.Create(track, progressSeconds);
                            }
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

            return result;
        }
    }
}
