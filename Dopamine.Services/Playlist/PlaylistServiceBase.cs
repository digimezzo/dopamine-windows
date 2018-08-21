using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using System;
using System.IO;
using System.Timers;

namespace Dopamine.Services.Playlist
{
    public class PlaylistServiceBase
    {
        private Timer playlistFolderChangedTimer = new Timer();

        public string PlaylistFolder { get; }

        public FileSystemWatcher Watcher { get; } = new FileSystemWatcher();

        public PlaylistServiceBase()
        {
            // Initialize Playlists folder
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            this.PlaylistFolder = Path.Combine(musicFolder, ProductInformation.ApplicationName, "Playlists");

            if (!Directory.Exists(this.PlaylistFolder))
            {
                try
                {
                    Directory.CreateDirectory(this.PlaylistFolder);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not create Playlists folder. Exception: {0}", ex.Message);
                }
            }

            // Set watcher path
            this.Watcher.Path = this.PlaylistFolder;

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
            this.Watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Only watch m3u files
            this.Watcher.Filter = "*" + FileFormats.M3U;

            // Add event handlers
            this.Watcher.Changed += new FileSystemEventHandler(OnChanged);
            this.Watcher.Created += new FileSystemEventHandler(OnChanged);
            this.Watcher.Deleted += new FileSystemEventHandler(OnChanged);
            this.Watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching
            this.Watcher.EnableRaisingEvents = true;

            // Configure timer
            playlistFolderChangedTimer.Interval = 250;
            playlistFolderChangedTimer.Elapsed += PlaylistsChangedTimer_Elapsed;
            playlistFolderChangedTimer.Start();
        }

        public event EventHandler PlaylistFolderChanged = delegate { };

        private void PlaylistsChangedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            playlistFolderChangedTimer.Stop();
            this.PlaylistFolderChanged(this, new EventArgs());
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            playlistFolderChangedTimer.Stop();
            playlistFolderChangedTimer.Start();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            playlistFolderChangedTimer.Stop();
            playlistFolderChangedTimer.Start();
        }
    }
}
