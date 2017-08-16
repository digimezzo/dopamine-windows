using Dopamine.Core.Logging;
using Dopamine.Core.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Core.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Dopamine.Core.Database;

namespace Dopamine.Common.Database.Repositories
{
    public class TrackStatisticRepository : ITrackStatisticRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public TrackStatisticRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

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
                            TrackStatistic existingTrackStatistic = conn.Query<TrackStatistic>("SELECT * FROM TrackStatistic WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();

                            if (existingTrackStatistic != null)
                            {
                                existingTrackStatistic.Rating = rating;
                                conn.Update(existingTrackStatistic);
                            }
                            else
                            {
                                var newTrackStatistic = new TrackStatistic();
                                newTrackStatistic.Path = path;
                                newTrackStatistic.SafePath = path.ToSafePath();
                                newTrackStatistic.Rating = rating;
                                conn.Insert(newTrackStatistic);
                            }
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
        public async Task UpdateLoveAsync(string path, bool love)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            TrackStatistic existingTrackStatistic = conn.Query<TrackStatistic>("SELECT * FROM TrackStatistic WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();

                            if (existingTrackStatistic != null)
                            {
                                existingTrackStatistic.Love = love ? 1 : 0;
                                conn.Update(existingTrackStatistic);
                            }
                            else
                            {
                                var newTrackStatistic = new TrackStatistic();
                                newTrackStatistic.Path = path;
                                newTrackStatistic.SafePath = path.ToSafePath();
                                newTrackStatistic.Love = love ? 1 : 0;
                                conn.Insert(newTrackStatistic);
                            }
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

        public async Task UpdateTrackStatisticAsync(TrackStatistic trackStatistic)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            TrackStatistic existingTrackStatistic = conn.Query<TrackStatistic>("SELECT * FROM TrackStatistic WHERE SafePath=?", trackStatistic.Path.ToSafePath()).FirstOrDefault();

                            if (existingTrackStatistic != null)
                            {
                                existingTrackStatistic.PlayCount = trackStatistic.PlayCount;
                                existingTrackStatistic.SkipCount = trackStatistic.SkipCount;
                                existingTrackStatistic.DateLastPlayed = trackStatistic.DateLastPlayed;
                                conn.Update(existingTrackStatistic);
                            }
                            else
                            {
                                conn.Insert(trackStatistic);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update statistics for path='{0}'. Exception: {1}", trackStatistic.Path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }

        public async Task<TrackStatistic> GetTrackStatisticAsync(string path)
        {
            TrackStatistic trackStatistic = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            trackStatistic = conn.Query<TrackStatistic>("SELECT * FROM TrackStatistic WHERE SafePath=?", path.ToSafePath()).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get TrackStatistic for path='{0}'. Exception: {1}", path, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return trackStatistic;
        }
    }
}
