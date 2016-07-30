using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class PlaylistEntryRepository : IPlaylistEntryRepository
    {
        #region IPlaylistEntryRepository
        public async Task DeleteOrphanedPlaylistEntriesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            db.PlaylistEntries.RemoveRange(db.PlaylistEntries.Where((p) => !db.Tracks.Select((t) => t.TrackID).Distinct().Contains(p.TrackID)));
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("There was a problem while deleting orphaned PlaylistEntries. Exception: {0}", ex.Message);
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
