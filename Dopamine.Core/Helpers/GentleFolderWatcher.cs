using System;
using System.IO;
using System.Timers;

namespace Dopamine.Core.Helpers
{
    /// <summary>
    /// A folder watcher that is not too nervous when notifying of changes
    /// </summary>
    public class GentleFolderWatcher
    {
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private Timer changeNotificationTimer = new Timer();
        private double changeNotificationTimeoutSeconds = 0.2;

        public event EventHandler FolderChanged = delegate { };

        public GentleFolderWatcher(string folderPath, bool includeSubdirectories)
        {
            // Timer
            this.changeNotificationTimer.Interval = TimeSpan.FromSeconds(this.changeNotificationTimeoutSeconds).TotalMilliseconds;
            this.changeNotificationTimer.Elapsed += new ElapsedEventHandler(ChangeNotificationTimerElapsed);

            // Set the folder to watch
            this.watcher.Path = folderPath;

            // Watch subdirectories or not
            this.watcher.IncludeSubdirectories = includeSubdirectories;

            // Add event handlers
            this.watcher.Changed += new FileSystemEventHandler(OnChanged);
            this.watcher.Created += new FileSystemEventHandler(OnChanged);
            this.watcher.Deleted += new FileSystemEventHandler(OnChanged);
            this.watcher.Renamed += new RenamedEventHandler(OnRenamed);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            this.changeNotificationTimer.Stop();
            this.changeNotificationTimer.Start();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            this.changeNotificationTimer.Stop();
            this.changeNotificationTimer.Start();
        }

        private void ChangeNotificationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.changeNotificationTimer.Stop();
            this.FolderChanged(this, new EventArgs());
        }

        public void Suspend()
        {
            this.watcher.EnableRaisingEvents = false;
            this.changeNotificationTimer.Stop();
        }

        public void Resume()
        {
            this.watcher.EnableRaisingEvents = true;
        }
    }
}