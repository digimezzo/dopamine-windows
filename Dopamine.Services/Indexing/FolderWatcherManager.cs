using Dopamine.Data.Entities;
using Dopamine.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Dopamine.Services.Indexing
{
    internal class FolderWatcherManager
    {
        private IFolderRepository folderRepository;
        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private Timer folderWatcherTimer;
       
        public event EventHandler FoldersChanged = delegate { };
      
        public FolderWatcherManager(IFolderRepository folderRepository)
        {
            this.folderRepository = folderRepository;
            folderWatcherTimer = new Timer(2000);
            folderWatcherTimer.Elapsed += FolderWatcherTimer_Elapsed;
        }
     
        private void FolderWatcherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.StopFolderWatcherTimer();

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.FoldersChanged(this, new EventArgs());
            });
        }

        private void ResetFolderWatcherTimer()
        {
            if (this.folderWatcherTimer.Enabled)
            {
                this.folderWatcherTimer.Stop();
            }

            this.folderWatcherTimer.Start();
        }

        private void StopFolderWatcherTimer()
        {
            if (this.folderWatcherTimer.Enabled)
            {
                this.folderWatcherTimer.Stop();
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.ResetFolderWatcherTimer();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            this.ResetFolderWatcherTimer();
        }

        public async Task StartWatchingAsync()
        {
            await this.StopWatchingAsync();

            List<Folder> folders = await this.folderRepository.GetFoldersAsync();

            foreach (Folder fol in folders)
            {
                if (Directory.Exists(fol.Path))
                {
                    var watcher = new FileSystemWatcher(fol.Path) { EnableRaisingEvents = true, IncludeSubdirectories = true };

                    watcher.Changed += Watcher_Changed;
                    watcher.Created += Watcher_Changed;
                    watcher.Deleted += Watcher_Changed;
                    watcher.Renamed += Watcher_Renamed;

                    this.watchers.Add(watcher);
                }
            }
        }

        public async Task StopWatchingAsync()
        {
            this.StopFolderWatcherTimer();

            if (this.watchers.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                for (int i = this.watchers.Count - 1; i >= 0; i--)
                {
                    this.watchers[i].Changed -= Watcher_Changed;
                    this.watchers[i].Created -= Watcher_Changed;
                    this.watchers[i].Deleted -= Watcher_Changed;
                    this.watchers[i].Renamed -= Watcher_Renamed;

                    this.watchers[i].Dispose();
                    this.watchers.RemoveAt(i);
                }
            });
        }
    }
}