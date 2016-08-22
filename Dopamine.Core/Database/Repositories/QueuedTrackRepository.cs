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
                            tracks = conn.Query<TrackInfo>("SELECT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path,"+
                                                           " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle,"+
                                                           " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year,"+
                                                           " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced,"+
                                                           " tra.DateFileModified, tra.MetaDataHash, art.ArtistName, gen.GenreName, alb.AlbumTitle," +
                                                           " alb.AlbumArtist, alb.AlbumYear, alb.AlbumArtworkID" +
                                                           " FROM QueuedTrack qtra" +
                                                           " INNER JOIN Track tra ON qtra.Path=tra.Path" +
                                                           " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                           " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                           " INNER JOIN Genre gen ON tra.GenreID=gen.GenreID"+
                                                           " ORDER BY qtra.OrderID");

        
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
                            conn.Execute("DELETE FROM QueuedTrack;");

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
