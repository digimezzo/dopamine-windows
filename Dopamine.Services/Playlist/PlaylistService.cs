using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
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

namespace Dopamine.Services.Playlist
{
    public class PlaylistService : PlaylistServiceBase, IPlaylistService
    {
        private IFileService fileService;
        private ITrackRepository trackRepository;
        private IContainerProvider container;

        public PlaylistService(IFileService fileService, ITrackRepository trackRepository, IContainerProvider container) : base()
        {
            this.fileService = fileService;
            this.trackRepository = trackRepository;
            this.container = container;
        }

        public event PlaylistAddedHandler PlaylistAdded = delegate { };
        public event PlaylistDeletedHandler PlaylistDeleted = delegate { };
        public event PlaylistRenamedHandler PlaylistRenamed = delegate { };
        public event TracksAddedHandler TracksAdded = delegate { };
        public event TracksDeletedHandler TracksDeleted = delegate { };

        private string CreatePlaylistFilename(string playlist)
        {
            return Path.Combine(this.PlaylistFolder, playlist + FileFormats.M3U);
        }

        public async Task<string> GetUniquePlaylistAsync(string proposedPlaylistName)
        {
            string uniquePlaylist = proposedPlaylistName;

            try
            {
                string[] filenames = Directory.GetFiles(this.PlaylistFolder);

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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            if (result == DeletePlaylistsResult.Success)
            {
                this.PlaylistDeleted(playlistName);
            }

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            if (result == RenamePlaylistResult.Success)
            {
                this.PlaylistRenamed(oldPlaylistName, sanitizedNewPlaylist);
            }

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }

        public async Task<IList<string>> GetPlaylistsAsync()
        {
            var playlists = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.PlaylistFolder);
                    var fi = di.GetFiles("*" + FileFormats.M3U, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        playlists.Add(Path.GetFileNameWithoutExtension(f.FullName));
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists;
        }

        private async Task<ImportPlaylistResult> ImportPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error("FileName is empty");
                return ImportPlaylistResult.Error;
            }

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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
                playlistName = await this.GetUniquePlaylistAsync(System.IO.Path.GetFileNameWithoutExtension(fileName));
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while getting unique playlist filename. Exception: {0}", ex.Message);
                return ImportPlaylistResult.Error;
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
                return ImportPlaylistResult.Error;
            }

            // If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success, so we can always raise the PlaylistAdded Event.
            this.PlaylistAdded(playlistName);

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return ImportPlaylistResult.Success;
        }

        public async Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames)
        {
            ImportPlaylistResult finalResult = ImportPlaylistResult.Success;

            foreach (string fileName in fileNames)
            {
                ImportPlaylistResult result = await this.ImportPlaylistAsync(fileName);

                if (!result.Equals(ImportPlaylistResult.Success))
                {
                    finalResult = result;
                }
            }

            return finalResult;
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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder
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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = false; // Stop watching the playlist folder

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

            this.Watcher.EnableRaisingEvents = true; // Start watching the playlist folder

            return result;
        }
    }
}
