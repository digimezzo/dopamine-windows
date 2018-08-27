using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Entities;
using Dopamine.Services.Extensions;
using Dopamine.Services.File;
using Dopamine.Services.Utils;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Services.Playlist
{
    public class PlaylistService : PlaylistServiceBase, IPlaylistService
    {
        private IFileService fileService;
        private ITrackRepository trackRepository;
        private IContainerProvider container;

        public override string PlaylistFolder { get; }

        public override string DialogFileFilter => $"(*{FileFormats.M3U};*{FileFormats.WPL};*{FileFormats.ZPL})|*{FileFormats.M3U};*{FileFormats.WPL};*{FileFormats.ZPL}";

        public PlaylistService(IFileService fileService, ITrackRepository trackRepository, IContainerProvider container) : base()
        {
            // Dependency injection
            this.fileService = fileService;
            this.trackRepository = trackRepository;
            this.container = container;

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

        public event TracksAddedHandler TracksAdded = delegate { };
        public event TracksDeletedHandler TracksDeleted = delegate { };

        private string CreatePlaylistFilename(string playlist)
        {
            return Path.Combine(this.PlaylistFolder, playlist + FileFormats.M3U);
        }

        public async Task<string> GetUniquePlaylistNameAsync(string proposedPlaylistName)
        {
            string uniquePlaylistName = proposedPlaylistName;

            try
            {
                await Task.Run(() =>
                {
                    string[] filenames = Directory.GetFiles(this.PlaylistFolder);
                    IList<string> existingPlaylistNames = filenames.Select(f => System.IO.Path.GetFileNameWithoutExtension(f)).ToList();
                    uniquePlaylistName = proposedPlaylistName.MakeUnique(existingPlaylistNames);
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not generate unique playlist name for playlist '{0}'. Exception: {1}", proposedPlaylistName, ex.Message);
            }

            return uniquePlaylistName;
        }

        public override async Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return AddPlaylistResult.Blank;
            }

            string sanitizedPlaylistName = FileUtils.SanitizeFilename(playlistName);
            string filename = this.CreatePlaylistFilename(sanitizedPlaylistName);

            if (System.IO.File.Exists(filename))
            {
                return AddPlaylistResult.Duplicate;
            }

            AddPlaylistResult result = AddPlaylistResult.Success;

            this.Watcher.Suspend(); // Stop watching the playlist folder

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

            if (result == AddPlaylistResult.Success)
            {
                this.OnPlaylistAdded(new PlaylistViewModel(sanitizedPlaylistName, filename));
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public override async Task<IList<PlaylistViewModel>> GetPlaylistsAsync()
        {
            IList<PlaylistViewModel> playlists = new List<PlaylistViewModel>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.PlaylistFolder);
                    FileInfo[] fi = di.GetFiles("*" + FileFormats.M3U, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        playlists.Add(new PlaylistViewModel(Path.GetFileNameWithoutExtension(f.FullName), f.FullName));
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists;
        }

        protected override async Task<ImportPlaylistResult> ImportPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error("FileName is empty");
                return ImportPlaylistResult.Error;
            }

            this.Watcher.Suspend(); // Stop watching the playlist folder

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
                return ImportPlaylistResult.Error;
            }

            // Set the paths
            // -------------
            paths = decodeResult.Paths;

            // Get a unique name for the playlist
            // ----------------------------------
            try
            {
                playlistName = await this.GetUniquePlaylistNameAsync(System.IO.Path.GetFileNameWithoutExtension(fileName));
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while getting unique playlist filename. Exception: {0}", ex.Message);
                return ImportPlaylistResult.Error;
            }

            // Create the Playlist in the playlists folder
            // -------------------------------------------
            string sanitizedPlaylistName = FileUtils.SanitizeFilename(playlistName);
            string filename = this.CreatePlaylistFilename(sanitizedPlaylistName);

            ImportPlaylistResult result = ImportPlaylistResult.Success;

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
                result = ImportPlaylistResult.Error;
            }

            if (result.Equals(ImportPlaylistResult.Success))
            {
                this.OnPlaylistAdded(new PlaylistViewModel(sanitizedPlaylistName, filename));
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public async Task<IList<TrackViewModel>> GetTracks(string playlistName)
        {
            // If no playlist was selected, return no tracks.
            if (string.IsNullOrEmpty(playlistName))
            {
                LogClient.Error("PlaylistName is empty. Returning empty list of tracks.");
                return new List<TrackViewModel>();
            }

            var tracks = new List<TrackViewModel>();
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

        public async Task SetPlaylistOrderAsync(IList<TrackViewModel> tracks, string playlistName)
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

            this.Watcher.Suspend(); // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    string filename = this.CreatePlaylistFilename(playlistName);

                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            foreach (TrackViewModel track in tracks)
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

            this.Watcher.Resume(); // Start watching the playlist folder
        }

        public async Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackViewModel> tracks, string playlistName)
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

            this.Watcher.Suspend(); // Stop watching the playlist folder

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
                            foreach (TrackViewModel track in tracks)
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

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<string> artists, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetArtistTracksAsync(artists);
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(orderedTracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<string> genres, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetGenreTracksAsync(genres);
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(orderedTracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetAlbumTracksAsync(albumViewModels.Select(x => x.AlbumKey).ToList());
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToPlaylistAsync(orderedTracks, playlistName);

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

            this.Watcher.Suspend(); // Stop watching the playlist folder

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

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public override async Task<RenamePlaylistResult> RenamePlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName)
        {
            if (playlistToRename == null)
            {
                LogClient.Error($"{nameof(playlistToRename)} is null");
                return RenamePlaylistResult.Error;
            }
            if (string.IsNullOrWhiteSpace(newPlaylistName))
            {
                LogClient.Error($"{nameof(newPlaylistName)} is empty");
                return RenamePlaylistResult.Blank;
            }

            string oldFilename = playlistToRename.Path;

            if (!System.IO.File.Exists(oldFilename))
            {
                LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", playlistToRename.Path);
                return RenamePlaylistResult.Error;
            }

            string sanitizedNewPlaylistName = FileUtils.SanitizeFilename(newPlaylistName);
            string newFilename = this.CreatePlaylistFilename(sanitizedNewPlaylistName);

            if (System.IO.File.Exists(newFilename))
            {
                return RenamePlaylistResult.Duplicate;
            }

            RenamePlaylistResult result = RenamePlaylistResult.Success;

            this.Watcher.Suspend(); // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    System.IO.File.Move(oldFilename, newFilename);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while renaming playlist '{0}' to '{1}'. Exception: {2}", playlistToRename.Name, newPlaylistName, ex.Message);
                    result = RenamePlaylistResult.Error;
                }
            });

            if (result == RenamePlaylistResult.Success)
            {
                this.OnPlaylistRenamed(
                    playlistToRename,
                    new PlaylistViewModel(sanitizedNewPlaylistName, newFilename));
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }
    }
}
