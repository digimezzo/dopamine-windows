using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Services.Entities;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistService : PlaylistServiceBase, ISmartPlaylistService
    {
        public override string PlaylistFolder { get; }

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
            this.Watcher = new GentleFolderWatcher(this.PlaylistFolder, false);
            this.Watcher.FolderChanged += Watcher_FolderChanged;
            this.Watcher.Resume();
        }

        private void Watcher_FolderChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.OnPlaylistFolderChanged(this);
            });
        }

        private string GetPlaylistName(string smartPlaylistPath)
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

        private void SetPlaylistNameIfDifferent(string smartPlaylistPath, string newPlaylistName)
        {
            string name = string.Empty;

            try
            {
                XDocument xdoc = XDocument.Load(smartPlaylistPath);

                XElement nameElement = (from t in xdoc.Element("smartplaylist").Elements("name")
                                        select t).FirstOrDefault();

                if (!nameElement.Value.Equals(newPlaylistName))
                {
                    nameElement.Value = newPlaylistName;
                    xdoc.Save(smartPlaylistPath);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not set name for smart playlist '{smartPlaylistPath}', new playlist name '{newPlaylistName}'. Exception: {ex.Message}");
            }
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
                        string name = this.GetPlaylistName(f.FullName);

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

        private string CreatePlaylistFilename(string playlist)
        {
            return Path.Combine(this.PlaylistFolder, playlist + FileFormats.DSPL);
        }

        protected override async Task<ImportPlaylistResult> ImportPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error($"{nameof(fileName)} is empty");
                return ImportPlaylistResult.Error;
            }

            IList<PlaylistViewModel> existingPlaylists = await this.GetPlaylistsAsync();

            string newPlaylistName = string.Empty;
            string newFileNameWithoutExtension = string.Empty;

            this.Watcher.Suspend(); // Stop watching the playlist folder

            ImportPlaylistResult result = ImportPlaylistResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    IList<string> existingPlaylistNames = existingPlaylists.Select(x => x.Name).ToList();
                    IList<string> existingFileNamesWithoutExtension = existingPlaylists.Select(x => Path.GetFileNameWithoutExtension(x.Path)).ToList();

                    string originalPlaylistName = this.GetPlaylistName(fileName);
                    newPlaylistName = originalPlaylistName.MakeUnique(existingPlaylistNames);

                    string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    newFileNameWithoutExtension = originalFileNameWithoutExtension.MakeUnique(existingFileNamesWithoutExtension);

                    // Generate a new filename for the playlist
                    string newPlaylistFileName = this.CreatePlaylistFilename(newFileNameWithoutExtension);

                    // Copy the playlist file to the playlists folder, using the new filename.
                    System.IO.File.Copy(fileName, newPlaylistFileName);

                    // Change the playlist name to the unique name (if changed)
                    this.SetPlaylistNameIfDifferent(newPlaylistFileName, newPlaylistName);
                }
                catch (Exception ex)
                {
                    LogClient.Error($"Error while importing smart playlist. Exception: {ex.Message}");
                    result = ImportPlaylistResult.Error;
                }
            });

            if (result.Equals(ImportPlaylistResult.Success))
            {
                this.OnPlaylistAdded(new PlaylistViewModel(newPlaylistName, newFileNameWithoutExtension));
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            throw new NotImplementedException();
        }

        public Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            throw new NotImplementedException();
        }
    }
}
