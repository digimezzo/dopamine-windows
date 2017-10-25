using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.IO;
using Dopamine.Common.Services.File;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Common.Services.Playlist
{
    public class PlaylistService : IPlaylistService
    {
        private IFileService fileService;
        private ITrackRepository trackRepository;
        private string playlistFolder;
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private Timer playlistFolderChangedTimer = new Timer();
     
        public string PlaylistFolder
        {
            get { return this.playlistFolder; }
        }
     
        public PlaylistService(IFileService fileService, ITrackRepository trackRepository)
        {
            // Services
            this.fileService = fileService;

            // Repositories
            this.trackRepository = trackRepository;

            // Initialize Playlists folder
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            this.playlistFolder = Path.Combine(musicFolder, ProductInformation.ApplicationName, "Playlists");

            if (!Directory.Exists(playlistFolder))
            {
                try
                {
                    Directory.CreateDirectory(playlistFolder);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not create Playlists folder. Exception: {0}", ex.Message);
                }
            }

            // Set watcher path
            watcher.Path = playlistFolder;

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            
            // Only watch m3u files
            watcher.Filter = "*" + FileFormats.M3U;

            // Add event handlers
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching
            watcher.EnableRaisingEvents = true;

            // Configure timer
            playlistFolderChangedTimer.Interval = 250;
            playlistFolderChangedTimer.Elapsed += PlaylistsChangedTimer_Elapsed;
            playlistFolderChangedTimer.Start();
        }
     
        public event PlaylistAddedHandler PlaylistAdded = delegate { };
        public event PlaylistDeletedHandler PlaylistDeleted = delegate { };
        public event PlaylistRenamedHandler PlaylistRenamed = delegate { };
        public event TracksAddedHandler TracksAdded = delegate { };
        public event TracksDeletedHandler TracksDeleted = delegate { };
        public event EventHandler PlaylistFolderChanged = delegate { };
    
        private string CreatePlaylistFilename(string playlist)
        {
            return Path.Combine(this.playlistFolder, playlist + FileFormats.M3U);
        }
    
        public async Task<string> GetUniquePlaylistAsync(string proposedPlaylistName)
        {
            string uniquePlaylist = proposedPlaylistName;

            try
            {
                string[] filenames = Directory.GetFiles(this.playlistFolder);

                List<string> existingPlaylists = filenames.Select(f => System.IO.Path.GetFileNameWithoutExtension(f)).ToList();

                await Task.Run(() =>
                {
                    int number = 1;

                    while (existingPlaylists.Contains(uniquePlaylist))
                    {
                        number++;
                        uniquePlaylist = proposedPlaylistName + " (" + number + ")";
                    }
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not generate unique playlist name for playlist '{0}'. Exception: {1}", proposedPlaylistName, ex.Message);
            }

            return uniquePlaylist;
        }

        public async Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName)) return AddPlaylistResult.Blank;

            string sanitizedPlaylistName = FileUtils.SanitizeFilename(playlistName);
            string filename = this.CreatePlaylistFilename(sanitizedPlaylistName);
            if (System.IO.File.Exists(filename)) return AddPlaylistResult.Duplicate;

            AddPlaylistResult result = AddPlaylistResult.Success;

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    System.IO.File.Create(filename).Close(); // Close() prevents file in use issues
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not create playlist '{0}' with filename '{1}'. Exception: {2}", playlistName, filename, ex.Message);
                    result = AddPlaylistResult.Error;
                }
            });

            if (result == AddPlaylistResult.Success) this.PlaylistAdded(sanitizedPlaylistName);

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }

        public async Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                LogClient.Error("PlaylistName is empty");
                return DeletePlaylistsResult.Error;
            }

            DeletePlaylistsResult result = DeletePlaylistsResult.Success;

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    string filename = this.CreatePlaylistFilename(playlistName);

                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }
                    else
                    {
                        result = DeletePlaylistsResult.Error;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while deleting playlist '{0}'. Exception: {1}", playlistName, ex.Message);
                    result = DeletePlaylistsResult.Error;
                }
            });

            if (result == DeletePlaylistsResult.Success) this.PlaylistDeleted(playlistName);

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }

        public async Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            if (string.IsNullOrWhiteSpace(oldPlaylistName))
            {
                LogClient.Error("OldPlaylistName is empty");
                return RenamePlaylistResult.Error;
            }
            if (string.IsNullOrWhiteSpace(newPlaylistName))
            {
                LogClient.Error("NewPlaylistName is empty");
                return RenamePlaylistResult.Blank;
            }

            string oldFilename = this.CreatePlaylistFilename(oldPlaylistName);
            if (!System.IO.File.Exists(oldFilename))
            {
                LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", oldPlaylistName);
                return RenamePlaylistResult.Error;
            }

            string sanitizedNewPlaylist = FileUtils.SanitizeFilename(newPlaylistName);
            string newFilename = this.CreatePlaylistFilename(sanitizedNewPlaylist);
            if (System.IO.File.Exists(newFilename)) return RenamePlaylistResult.Duplicate;

            RenamePlaylistResult result = RenamePlaylistResult.Success;

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    System.IO.File.Move(oldFilename, newFilename);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while renaming playlist '{0}' to '{1}'. Exception: {2}", oldPlaylistName, newPlaylistName, ex.Message);
                    result = RenamePlaylistResult.Error;
                }
            });

            if (result == RenamePlaylistResult.Success) this.PlaylistRenamed(oldPlaylistName, sanitizedNewPlaylist);

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }

        public async Task<List<string>> GetPlaylistsAsync()
        {
            var playlists = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.playlistFolder);
                    var fi = di.GetFiles("*" + FileFormats.M3U, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        playlists.Add(Path.GetFileNameWithoutExtension(f.FullName));
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting playlist. Exception: {0}", ex.Message);
                }
            });

            return playlists;
        }

        public async Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error("FileName is empty");
                return OpenPlaylistResult.Error;
            }

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            string playlistName = String.Empty;
            var paths = new List<String>();

            // Decode the playlist file
            // ------------------------
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult decodeResult = null;

            await Task.Run(() => decodeResult = decoder.DecodePlaylist(fileName));

            if (!decodeResult.DecodeResult.Result)
            {
                LogClient.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
                return OpenPlaylistResult.Error;
            }

            // Set the paths
            // -------------
            paths = decodeResult.Paths;

            // Get a unique name for the playlist
            // ----------------------------------
            try
            {
                playlistName = await this.GetUniquePlaylistAsync(System.IO.Path.GetFileNameWithoutExtension(fileName));
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while getting unique playlist filename. Exception: {0}", ex.Message);
                return OpenPlaylistResult.Error;
            }

            // Create the Playlist in the playlists folder
            // -------------------------------------------
            string sanitizedPlaylist = FileUtils.SanitizeFilename(playlistName);
            string filename = this.CreatePlaylistFilename(sanitizedPlaylist);

            try
            {
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    using (var writer = new StreamWriter(fs))
                    {
                        foreach (string path in paths)
                        {
                            try
                            {

                                writer.WriteLine(path);
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("Could not write path '{0}' to playlist '{1}' with filename '{2}'. Exception: {3}", path, playlistName, filename, ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not create playlist '{0}' with filename '{1}'. Exception: {2}", playlistName, filename, ex.Message);
                return OpenPlaylistResult.Error;
            }

            // If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success, so we can always raise the PlaylistAdded Event.
            this.PlaylistAdded(playlistName);

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return OpenPlaylistResult.Success;
        }

        public async Task<List<PlayableTrack>> GetTracks(string playlistName)
        {
            // If no playlist was selected, return no tracks.
            if (string.IsNullOrEmpty(playlistName))
            {
                LogClient.Error("PlaylistName is empty. Returning empty list of tracks.");
                return new List<PlayableTrack>();
            }

            var tracks = new List<PlayableTrack>();
            var decoder = new PlaylistDecoder();

            await Task.Run(async () =>
            {
                string filename = this.CreatePlaylistFilename(playlistName);
                DecodePlaylistResult decodeResult = null;
                decodeResult = decoder.DecodePlaylist(filename);

                if (decodeResult.DecodeResult.Result)
                {
                    foreach (string path in decodeResult.Paths)
                    {
                        try
                        {
                            tracks.Add(await this.fileService.CreateTrackAsync(path));
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get track information from file. Exception: {0}", ex.Message);
                        }
                    }
                }
            });

            return tracks;
        }

        public async Task SetPlaylistOrderAsync(IList<PlayableTrack> tracks, string playlistName)
        {
            if (tracks == null || tracks.Count == 0)
            {
                LogClient.Error("Cannot set playlist order. No tracks were provided.");
                return;
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                LogClient.Error("Cannot set playlist order. No playlistName was provided.");
                return;
            }

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    string filename = this.CreatePlaylistFilename(playlistName);

                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            foreach (PlayableTrack track in tracks)
                            {
                                sw.WriteLine(track.Path);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not set the playlist order. Exception: {0}", ex.Message);
                }
            });

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder
        }

        public async Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlistName)
        {
            if (tracks == null || tracks.Count == 0)
            {
                LogClient.Error("Cannot add tracks to playlist. No tracks were provided.");
                return AddTracksToPlaylistResult.Error;
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                LogClient.Error("Cannot add tracks to playlist. No playlistName was provided.");
                return AddTracksToPlaylistResult.Error;
            }

            AddTracksToPlaylistResult result = AddTracksToPlaylistResult.Success;

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            int numberTracksAdded = 0;
            string filename = this.CreatePlaylistFilename(playlistName);

            await Task.Run(() =>
            {
                try
                {
                    using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
                    {
                        using (var writer = new StreamWriter(fs))
                        {
                            foreach (PlayableTrack track in tracks)
                            {
                                try
                                {
                                    writer.WriteLine(track.Path);
                                    numberTracksAdded++;
                                }
                                catch (Exception ex)
                                {
                                    LogClient.Error("Could not write path '{0}' to playlist '{1}' with filename '{2}'. Exception: {3}", track.Path, playlistName, filename, ex.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not add tracks to playlist '{0}' with filename '{1}'. Exception: {2}", playlistName, filename, ex.Message);
                    result = AddTracksToPlaylistResult.Error;
                }
            });

            if (result == AddTracksToPlaylistResult.Success) this.TracksAdded(numberTracksAdded, playlistName);

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(tracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(tracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName)
        {
            List<PlayableTrack> tracks = await DatabaseUtils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(tracks, playlistName);

            return result;
        }

        public async Task<DeleteTracksFromPlaylistResult> DeleteTracksFromPlaylistAsync(IList<int> indexes, string playlistName)
        {
            if (indexes == null || indexes.Count == 0)
            {
                LogClient.Error("Cannot delete tracks from playlist. No indexes were provided.");
                return DeleteTracksFromPlaylistResult.Error;
            }

            if (string.IsNullOrEmpty(playlistName))
            {
                LogClient.Error("Cannot delete tracks from playlist. No playlistName was provided.");
                return DeleteTracksFromPlaylistResult.Error;
            }

            DeleteTracksFromPlaylistResult result = DeleteTracksFromPlaylistResult.Success;

            watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

            string filename = this.CreatePlaylistFilename(playlistName);
           
            var builder = new StringBuilder();

            string line = null;

            await Task.Run(() =>
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        int lineIndex = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!indexes.Contains(lineIndex))
                            {
                                builder.AppendLine(line);
                            }

                            lineIndex++;
                        }
                    }

                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        using (var writer = new StreamWriter(fs))
                        {
                            writer.Write(builder.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not delete tracks from playlist '{0}' with filename '{1}'. Exception: {2}", playlistName, filename, ex.Message);
                    result = DeleteTracksFromPlaylistResult.Error;
                }
            });

            if (result == DeleteTracksFromPlaylistResult.Success)
            {
                this.TracksDeleted(playlistName);
            }

            watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }
   
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
