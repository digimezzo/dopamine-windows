using Digimezzo.Utilities.Log;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task UpdateCountersAsync(string path, int playCount, int skipCount, long dateLastPlayed)
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
                                if (existingTrackStatistic.PlayCount.HasValue)
                                {
                                    existingTrackStatistic.PlayCount += playCount;
                                }
                                else
                                {
                                    existingTrackStatistic.PlayCount = playCount;
                                }

                                if (existingTrackStatistic.SkipCount.HasValue)
                                {
                                    existingTrackStatistic.SkipCount += skipCount;
                                }
                                else
                                {
                                    existingTrackStatistic.SkipCount = skipCount;
                                }

                                existingTrackStatistic.DateLastPlayed = dateLastPlayed;
                                conn.Update(existingTrackStatistic);
                            }
                            else
                            {
                                var newTrackStatistic = new TrackStatistic();
                                newTrackStatistic.Path = path;
                                newTrackStatistic.SafePath = path.ToSafePath();
                                newTrackStatistic.PlayCount = playCount;
                                newTrackStatistic.SkipCount = skipCount;
                                newTrackStatistic.DateLastPlayed = dateLastPlayed;
                                conn.Insert(newTrackStatistic);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update counters for path='{0}'. Exception: {1}", path, ex.Message);
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
