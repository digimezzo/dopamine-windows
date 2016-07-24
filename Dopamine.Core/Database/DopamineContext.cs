using Dopamine.Core.Database.Entities;
using Dopamine.Core.Settings;
using System;
using System.Data.Entity;

namespace Dopamine.Core.Database
{
    public class DopamineContext : DbContext
    {
        #region Properties
        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistEntry> PlaylistEntries { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<RemovedTrack> RemovedTracks { get; set; }
        public DbSet<QueuedTrack> QueuedTracks { get; set; }
        public DbSet<IndexingStatistic> IndexingStatistics { get; set; }
        #endregion

        #region Construction
        public DopamineContext()
        {
            // Set the "|DataDirectory|" path in the ConnectionString in App.config
            string dataDirectory = XmlSettingsClient.Instance.ApplicationFolder;
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
        }
        #endregion
    }
}
