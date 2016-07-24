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
        #region IQueuedTrackRepository
        public async Task<List<TrackInfo>> GetSavedQueuedTracksAsync()
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
                            tracks = (from qtra in db.QueuedTracks
                                      join tra in db.Tracks on qtra.Path equals tra.Path
                                      join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                      join art in db.Artists on tra.ArtistID equals art.ArtistID
                                      join gen in db.Genres on tra.GenreID equals gen.GenreID
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // First, clear old queued tracks
                            db.Database.ExecuteSqlCommand("DELETE FROM QueuedTracks;");

                            // Then, add new queued tracks
                            for (int index = 1; index <= tracks.Count; index++)
                            {
                                db.QueuedTracks.Add(new QueuedTrack
                                {
                                    OrderID = index,
                                    Path = tracks[index - 1].Path
                                });
                            }

                            db.SaveChanges();
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
