using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
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
        public async Task<List<TrackInfo>> GetSavedQueuedTracksAsync()
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
                            tracks = (from qtra in conn.Table<QueuedTrack>()
                                      join tra in conn.Table<Track>() on qtra.Path equals tra.Path
                                      join alb in conn.Table<Album>() on tra.AlbumID equals alb.AlbumID
                                      join art in conn.Table<Artist>() on tra.ArtistID equals art.ArtistID
                                      join gen in conn.Table<Genre>() on tra.GenreID equals gen.GenreID
                                      orderby qtra.OrderID
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
                            LogClient.Instance.Logger.Error("Could not get Queued Tracks. Exception: {0}", ex.Message);
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

        public async Task SaveQueuedTracksAsync(IList<Track> tracks)
        {
            if (tracks == null) return;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // First, clear old queued tracks
                            conn.Execute("DELETE FROM QueuedTracks;");

                            // Then, add new queued tracks
                            for (int index = 1; index <= tracks.Count; index++)
                            {
                                conn.Insert(new QueuedTrack
                                {
                                    OrderID = index,
                                    Path = tracks[index - 1].Path
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not save queued tracks. Exception: {0}", ex.Message);
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
