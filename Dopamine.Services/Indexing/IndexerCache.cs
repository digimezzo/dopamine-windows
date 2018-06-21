using Digimezzo.Utilities.Log;
using Dopamine.Data;
using Dopamine.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Indexing
{
    internal class IndexerCache
    {
        private HashSet<string> cachedTrackStatistics;
        private Dictionary<long, Track> cachedTracks;

        private ISQLiteConnectionFactory factory;

        public IndexerCache(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public bool HasCachedTrackStatistic(TrackStatistic trackStatistic)
        {
            if (this.cachedTrackStatistics.Contains(trackStatistic.SafePath))
            {
                return true;
            }

            this.cachedTrackStatistics.Add(trackStatistic.SafePath);

            return false;
        }

        public bool HasCachedTrack(ref Track track)
        {
            bool hasCachedTrack = false;
            long similarTrackId = 0;

            Track tempTrack = track;

            try
            {
                similarTrackId = this.cachedTracks.Where((t) => t.Value.Equals(tempTrack)).Select((t) => t.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Track with path '{0}' exists in the cache. Exception: {1}", track.Path, ex.Message);
            }

            if (similarTrackId != 0)
            {
                hasCachedTrack = true;
                track.TrackID = similarTrackId;
            }

            return hasCachedTrack;
        }

        public void AddTrack(Track track)
        {
            this.cachedTracks.Add(track.TrackID, track);
        }

        public void Initialize()
        {
            // Comparing new and existing objects will happen in a Dictionary cache. This should improve performance.
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                this.cachedTrackStatistics = new HashSet<string>(conn.Table<TrackStatistic>().ToList().Select(ts => ts.SafePath).ToList());
                this.cachedTracks = conn.Table<Track>().ToDictionary(trk => trk.TrackID, trk => trk);
            }
        }
    }
}
