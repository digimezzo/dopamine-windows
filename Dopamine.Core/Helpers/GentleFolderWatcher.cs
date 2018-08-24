using System;
using System.IO;
using System.Timers;
using System.Windows;

namespace Dopamine.Core.Helpers
{
    /// <summary>
    /// A folder watcher that is not too nervous when notifying of changes
    /// </summary>
    public class GentleFolderWatcher : IDisposable
    {
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private Timer changeNotificationTimer = new Timer();

        public event EventHandler FolderChanged = delegate { };

        public GentleFolderWatcher(string folderPath, bool includeSubdirectories, int intervalMilliSeconds = 200)
        {
            // Timer
            this.changeNotificationTimer.Interval = intervalMilliSeconds;
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.FolderChanged(this, new EventArgs());
            });
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

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   if(this.watcher != null)
                    {
                        this.watcher.EnableRaisingEvents = false;
                        this.changeNotificationTimer.Stop();
                        this.watcher.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}