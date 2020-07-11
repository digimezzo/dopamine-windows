using Dopamine.Data;
using Dopamine.Data.Entities;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Indexing
{
    internal class IndexerCache
    {
        private Dictionary<string, Track> cachedTracks;

        private readonly ISQLiteConnectionFactory factory;

        public IndexerCache(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public void Initialize()
        {
            // Comparing new and existing objects will happen in a Dictionary cache. This should improve performance.
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                this.cachedTracks = conn.Table<Track>().ToDictionary(trk => trk.SafePath, trk => trk);
            }
        }

        public Track GetTrack(string safePath)
        {
            lock(cachedTracks)
            {
                if (cachedTracks.TryGetValue(safePath, out Track track))
                {
                    return track;
                }
            }

            return null;
        }

        public void AddTrack(Track track)
        {
            lock(cachedTracks)
            {
                cachedTracks[track.SafePath] = track;
            }
        }
    }
}
