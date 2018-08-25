using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Services.Entities;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistService : ISmartPlaylistService
    {
        private GentleFolderWatcher watcher;

        public SmartPlaylistService()
        {
            // Initialize Playlists folder
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            this.PlaylistFolder = Path.Combine(musicFolder, ProductInformation.ApplicationName, "Smart playlists");

            if (!Directory.Exists(this.PlaylistFolder))
            {
                try
                {
                    Directory.CreateDirectory(this.PlaylistFolder);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not create Smart playlists folder. Exception: {0}", ex.Message);
                }
            }

            // Watcher
            this.watcher = new GentleFolderWatcher(this.PlaylistFolder, false);
            this.watcher.FolderChanged += Watcher_FolderChanged;
            this.watcher.Resume();
        }

        private void Watcher_FolderChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PlaylistFolderChanged(this, new EventArgs());
            });
        }

        public string PlaylistFolder { get; }

        public event PlaylistAddedHandler PlaylistAdded = delegate { };
        public event PlaylistDeletedHandler PlaylistDeleted = delegate { };
        public event PlaylistRenamedHandler PlaylistRenamed = delegate { };
        public event EventHandler PlaylistFolderChanged = delegate { };

        private string GetSmartPlaylistName(string smartPlaylistPath)
        {
            string name = string.Empty;

            try
            {
                XDocument xdoc = XDocument.Load(smartPlaylistPath);

                XElement nameElement = (from t in xdoc.Element("smartplaylist").Elements("name")
                                        select t).FirstOrDefault();

                name = nameElement.Value;
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not get name for smart playlist '{smartPlaylistPath}'. Exception: {ex.Message}");
            }

            return name;
        }

        public async Task<IList<PlaylistViewModel>> GetPlaylistsAsync()
        {
            IList<PlaylistViewModel> playlists = new List<PlaylistViewModel>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.PlaylistFolder);
                    var fi = di.GetFiles("*" + FileFormats.DSPL, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        string name = this.GetSmartPlaylistName(f.FullName);

                        if (!string.IsNullOrEmpty(name))
                        {
                            playlists.Add(new PlaylistViewModel(name, f.FullName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting smart playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists;
        }

        public Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames)
        {
            throw new NotImplementedException();
        }

        public Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            throw new NotImplementedException();
        }

        public Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlistName)
        {
            throw new NotImplementedException();
        }

        public Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            throw new NotImplementedException();
        }
    }
}
