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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Dopamine.Services.Playlist
{
    public class PlaylistService : IPlaylistService
    {
        private ITrackRepository trackRepository;
        private IContainerProvider container;
        private IFileService fileService;
        private GentleFolderWatcher watcher;

        public PlaylistService(ITrackRepository trackRepository,
            IFileService fileService, IContainerProvider container)
        {
            // Dependency injection
            this.trackRepository = trackRepository;
            this.fileService = fileService;
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
            this.watcher = new GentleFolderWatcher(this.PlaylistFolder, false);
            this.watcher.FolderChanged += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.PlaylistFolderChanged(this, new EventArgs());
                });
            };

            this.watcher.Resume();
        }

        public string PlaylistFolder { get; }

        public string DialogFileFilter => $"(*{FileFormats.M3U};*{FileFormats.WPL};*{FileFormats.ZPL};*{FileFormats.DSPL})|*{FileFormats.M3U};*{FileFormats.WPL};*{FileFormats.ZPL};*{FileFormats.DSPL};*{FileFormats.DSPL}";

        public event EventHandler PlaylistFolderChanged = delegate { };
        public event TracksAddedHandler TracksAdded = delegate { };
        public event TracksDeletedHandler TracksDeleted = delegate { };

        private string SanitizePlaylistFilename(string playlistName)
        {
            return FileUtils.SanitizeFilename(playlistName);
        }

        private string CreatePlaylistFilePath(string sanitizedPlaylistName, PlaylistType type)
        {
            string extension = type.Equals(PlaylistType.Smart) ? FileFormats.DSPL : FileFormats.M3U;
            return Path.Combine(this.PlaylistFolder, sanitizedPlaylistName + extension);
        }

        private async Task<PlaylistViewModel> CreateNewStaticPlaylistAsync(EditablePlaylistViewModel editablePlaylist)
        {
            if (editablePlaylist == null)
            {
                LogClient.Error($"{nameof(editablePlaylist)} is null");
                return null;
            }

            if (string.IsNullOrEmpty(editablePlaylist.Path))
            {
                LogClient.Error($"{nameof(editablePlaylist.Path)} is null or empty");
                return null;
            }

            try
            {
                await Task.Run(() =>
                {
                    // Close() prevents file in use issues
                    System.IO.File.Create(editablePlaylist.Path).Close();
                });
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not create playlist '{editablePlaylist.PlaylistName}' with filename '{editablePlaylist.Path}'. Exception: {ex.Message}");
                return null;
            }

            return new PlaylistViewModel(editablePlaylist.PlaylistName, editablePlaylist.Path, PlaylistType.Static);
        }

        private async Task<PlaylistViewModel> CreateNewSmartPlaylistAsync(EditablePlaylistViewModel editablePlaylist)
        {
            if (editablePlaylist == null)
            {
                LogClient.Error($"{nameof(editablePlaylist)} is null");
                return null;
            }

            if (string.IsNullOrEmpty(editablePlaylist.Path))
            {
                LogClient.Error($"{nameof(editablePlaylist.Path)} is null or empty");
                return null;
            }

            try
            {
                await Task.Run(() =>
                {
                    var decoder = new SmartPlaylistDecoder();
                    XDocument smartPlaylistDocument = decoder.EncodeSmartPlaylist(
                        editablePlaylist.PlaylistName,
                        editablePlaylist.MatchAnyRule,
                        editablePlaylist.Limit.ToSmartPlaylistLimit(),
                        editablePlaylist.Rules.Select(x => x.ToSmartPlaylistRule()).ToList());
                    smartPlaylistDocument.Save(editablePlaylist.Path);
                });
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not create playlist '{editablePlaylist.PlaylistName}' with filename '{editablePlaylist.Path}'. Exception: {ex.Message}");
                return null;
            }

            return new PlaylistViewModel(editablePlaylist.PlaylistName, editablePlaylist.Path, PlaylistType.Smart);
        }

        private async Task<PlaylistViewModel> UpdateSmartPlaylistAsync(EditablePlaylistViewModel editablePlaylist)
        {
            if (editablePlaylist == null)
            {
                LogClient.Error($"{nameof(editablePlaylist)} is null");
                return null;
            }

            if (string.IsNullOrEmpty(editablePlaylist.Path))
            {
                LogClient.Error($"{nameof(editablePlaylist.Path)} is null or empty");
                return null;
            }

            // Delete the old smart playlsit file
            await Task.Run(() =>
            {
                if (System.IO.File.Exists(editablePlaylist.Path))
                {
                    System.IO.File.Delete(editablePlaylist.Path);
                }
            });

            // Update the path of the smart playlist to create
            string sanitizedPlaylistName = this.SanitizePlaylistFilename(editablePlaylist.PlaylistName);
            editablePlaylist.Path = this.CreatePlaylistFilePath(sanitizedPlaylistName, editablePlaylist.Type);

            // Create a new smart playlist file
            return await this.CreateNewSmartPlaylistAsync(editablePlaylist);
        }

        public async Task<CreateNewPlaylistResult> CreateNewPlaylistAsync(EditablePlaylistViewModel editablePlaylist)
        {
            if (string.IsNullOrWhiteSpace(editablePlaylist.PlaylistName))
            {
                return CreateNewPlaylistResult.Blank;
            }

            string sanitizedPlaylistName = this.SanitizePlaylistFilename(editablePlaylist.PlaylistName);
            editablePlaylist.Path = this.CreatePlaylistFilePath(sanitizedPlaylistName, editablePlaylist.Type);

            if (System.IO.File.Exists(editablePlaylist.Path))
            {
                return CreateNewPlaylistResult.Duplicate;
            }

            this.watcher.Suspend(); // Stop watching the playlist folder

            PlaylistViewModel playlistViewModel = null;

            if (editablePlaylist.Type.Equals(PlaylistType.Static))
            {
                playlistViewModel = await this.CreateNewStaticPlaylistAsync(editablePlaylist);
            }
            else if (editablePlaylist.Type.Equals(PlaylistType.Smart))
            {
                playlistViewModel = await this.CreateNewSmartPlaylistAsync(editablePlaylist);
            }

            this.watcher.Resume(); // Start watching the playlist folder

            if (playlistViewModel == null)
            {
                return CreateNewPlaylistResult.Error;
            }

            this.PlaylistFolderChanged(this, new EventArgs());

            return CreateNewPlaylistResult.Success;
        }

        public async Task<AddTracksToPlaylistResult> AddTracksToStaticPlaylistAsync(IList<TrackViewModel> tracks, string playlistName)
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

            this.watcher.Suspend(); // Stop watching the playlist folder

            int numberTracksAdded = 0;
            string filename = this.CreatePlaylistFilePath(playlistName, PlaylistType.Static);

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

            if (result == AddTracksToPlaylistResult.Success)
            {
                this.TracksAdded(numberTracksAdded, playlistName);
            }

            this.watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddArtistsToStaticPlaylistAsync(IList<string> artists, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetArtistTracksAsync(artists);
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToStaticPlaylistAsync(orderedTracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddGenresToStaticPlaylistAsync(IList<string> genres, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetGenreTracksAsync(genres);
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToStaticPlaylistAsync(orderedTracks, playlistName);

            return result;
        }

        public async Task<AddTracksToPlaylistResult> AddAlbumsToStaticPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName)
        {
            IList<Track> tracks = await this.trackRepository.GetAlbumTracksAsync(albumViewModels.Select(x => x.AlbumKey).ToList());
            List<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(tracks.Select(t => this.container.ResolveTrackViewModel(t)).ToList(), TrackOrder.ByAlbum);
            AddTracksToPlaylistResult result = await this.AddTracksToStaticPlaylistAsync(orderedTracks, playlistName);

            return result;
        }

        public async Task<IList<PlaylistViewModel>> GetStaticPlaylistsAsync()
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
                        playlists.Add(new PlaylistViewModel(this.GetStaticPlaylistName(f.FullName), f.FullName, PlaylistType.Static));
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting static playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists.OrderBy(x => x.Name).ToList();
        }

        private string GetStaticPlaylistName(string staticPlaylistPath)
        {
            return Path.GetFileNameWithoutExtension(staticPlaylistPath);
        }

        private string GetSmartPlaylistName(string smartPlaylistPath)
        {
            var decoder = new SmartPlaylistDecoder();
            DecodeSmartPlaylistResult result = decoder.DecodePlaylist(smartPlaylistPath);

            return result.DecodeResult.Result ? result.PlaylistName : string.Empty;
        }

        public async Task<IList<PlaylistViewModel>> GetSmartPlaylistsAsync()
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
                            playlists.Add(new PlaylistViewModel(name, f.FullName, PlaylistType.Smart));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting smart playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists.OrderBy(x => x.Name).ToList();
        }

        public async Task<IList<PlaylistViewModel>> GetAllPlaylistsAsync()
        {
            // First, get the smart playlists.
            List<PlaylistViewModel> allPlaylists = (List<PlaylistViewModel>)await this.GetSmartPlaylistsAsync();

            // Then, add the static playlists.
            allPlaylists.AddRange(await this.GetStaticPlaylistsAsync());

            return allPlaylists;
        }

        private async Task<IList<TrackViewModel>> GetStaticPlaylistTracksAsync(PlaylistViewModel playlist)
        {
            var tracks = new List<TrackViewModel>();
            var decoder = new PlaylistDecoder();

            await Task.Run(async () =>
            {
                DecodePlaylistResult decodeResult = null;
                decodeResult = decoder.DecodePlaylist(playlist.Path);

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

        private async Task<IList<TrackViewModel>> GetSmartPlaylistTracksAsync(PlaylistViewModel playlist)
        {
            string whereClause = string.Empty;
            SmartPlaylistLimit limit = null;

            await Task.Run(() =>
            {
                var decoder = new SmartPlaylistDecoder();
                DecodeSmartPlaylistResult result = decoder.DecodePlaylist(playlist.Path);
                limit = result.Limit;
                var builder = new SmartPlaylistWhereClauseBuilder(result);
                whereClause = builder.GetWhereClause();
            });

            if (string.IsNullOrWhiteSpace(whereClause))
            {
                return new List<TrackViewModel>();
            }

            IList<Track> tracks = await this.trackRepository.GetTracksAsync(whereClause);
            IList<TrackViewModel> trackViewModels = new List<TrackViewModel>();

            try
            {
                // TODO: order by

                // No limit specified: add all tracks.
                if (limit.Value == 0)
                {
                    trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks);
                }
                else
                {
                    long sum = 0;

                    // Add a subset of the tracks, depending on the limit type.
                    switch (limit.Type)
                    {
                        case SmartPlaylistLimitType.Songs:
                            trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks.Take(limit.Value).ToList());
                            break;
                        case SmartPlaylistLimitType.GigaBytes:
                            trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks.TakeWhile(x => { long temp = sum; sum += x.FileSize.HasValue ? x.FileSize.Value : 0; return temp <= limit.Value * 1024 * 1024 * 1024; }).ToList());
                            break;
                        case SmartPlaylistLimitType.MegaBytes:
                            trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks.TakeWhile(x => { long temp = sum; sum += x.FileSize.HasValue ? x.FileSize.Value : 0; return temp <= limit.Value * 1024 * 1024; }).ToList());
                            break;
                        case SmartPlaylistLimitType.Minutes:
                            trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks.TakeWhile(x => { long temp = sum; sum += x.Duration.HasValue ? x.Duration.Value : 0; return temp <= TimeSpan.FromMinutes(limit.Value).Ticks; }).ToList());
                            break;
                        default:
                            trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error($"Unable to resolve TrackViewModels. Exception: {ex.Message}");
                return new List<TrackViewModel>();
            }

            return trackViewModels;
        }

        public async Task<IList<TrackViewModel>> GetTracksAsync(PlaylistViewModel playlist)
        {
            // If no playlist was selected, return no tracks.
            if (playlist == null)
            {
                LogClient.Error($"{nameof(playlist)} is null. Returning empty list of tracks.");
                return new List<TrackViewModel>();
            }

            IList<TrackViewModel> tracks = new List<TrackViewModel>();

            if (playlist.Type.Equals(PlaylistType.Static))
            {
                tracks = await this.GetStaticPlaylistTracksAsync(playlist);
            }
            else if (playlist.Type.Equals(PlaylistType.Smart))
            {
                tracks = await this.GetSmartPlaylistTracksAsync(playlist);
            }

            return tracks;
        }

        public async Task<DeleteTracksFromPlaylistResult> DeleteTracksFromStaticPlaylistAsync(IList<int> indexes, PlaylistViewModel playlist)
        {
            if (indexes == null || indexes.Count == 0)
            {
                LogClient.Error("Cannot delete tracks from playlist. No indexes were provided.");
                return DeleteTracksFromPlaylistResult.Error;
            }

            if (playlist == null)
            {
                LogClient.Error($"Cannot delete tracks from playlist. {nameof(playlist)} is null.");
                return DeleteTracksFromPlaylistResult.Error;
            }

            DeleteTracksFromPlaylistResult result = DeleteTracksFromPlaylistResult.Success;

            this.watcher.Suspend(); // Stop watching the playlist folder

            string filename = this.CreatePlaylistFilePath(playlist.Name, PlaylistType.Static);

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
                    LogClient.Error("Could not delete tracks from playlist '{0}' with filename '{1}'. Exception: {2}", playlist.Name, filename, ex.Message);
                    result = DeleteTracksFromPlaylistResult.Error;
                }
            });

            if (result == DeleteTracksFromPlaylistResult.Success)
            {
                this.TracksDeleted(playlist);
            }

            this.watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public async Task<string> GetUniquePlaylistNameAsync(string proposedPlaylistName)
        {
            string uniquePlaylistName = proposedPlaylistName;

            try
            {
                IList<PlaylistViewModel> staticPlaylists = await this.GetStaticPlaylistsAsync();
                IList<PlaylistViewModel> smartPlaylists = await this.GetSmartPlaylistsAsync();

                List<string> existingPlaylistNames = staticPlaylists.Select(x => x.Name).ToList();
                existingPlaylistNames.AddRange(smartPlaylists.Select(x => x.Name).ToList());

                uniquePlaylistName = proposedPlaylistName.MakeUnique(existingPlaylistNames);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not generate unique playlist name for playlist '{0}'. Exception: {1}", proposedPlaylistName, ex.Message);
            }

            return uniquePlaylistName;
        }

        private void SetSmartPlaylistNameIfDifferent(string smartPlaylistPath, string newPlaylistName)
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

        private async Task<EditPlaylistResult> EditStaticPlaylistAsync(EditablePlaylistViewModel editablePlaylistViewModel)
        {
            if (!System.IO.File.Exists(editablePlaylistViewModel.Path))
            {
                LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", editablePlaylistViewModel.Path);
                return EditPlaylistResult.Error;
            }

            string sanitizedNewPlaylistName = this.SanitizePlaylistFilename(editablePlaylistViewModel.PlaylistName);
            string newFilename = this.CreatePlaylistFilePath(sanitizedNewPlaylistName, PlaylistType.Static);

            if (System.IO.File.Exists(newFilename))
            {
                return EditPlaylistResult.Duplicate;
            }

            EditPlaylistResult result = EditPlaylistResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    System.IO.File.Move(editablePlaylistViewModel.Path, newFilename);
                }
                catch (Exception ex)
                {
                    LogClient.Error($"Error while editing static playlist '{editablePlaylistViewModel.Path}'. Exception: {ex.Message}");
                    result = EditPlaylistResult.Error;
                }
            });

            return result;
        }

        private async Task<EditPlaylistResult> EditSmartPlaylistAsync(EditablePlaylistViewModel editablePlaylistViewModel)
        {
            if (!System.IO.File.Exists(editablePlaylistViewModel.Path))
            {
                LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", editablePlaylistViewModel.Path);
                return EditPlaylistResult.Error;
            }

            IList<PlaylistViewModel> existingSmartPlaylists = await this.GetSmartPlaylistsAsync();
            string oldSmartPlaylistName = this.GetSmartPlaylistName(editablePlaylistViewModel.Path);

            bool isEditingSamePlaylist = oldSmartPlaylistName.ToLower().Equals(editablePlaylistViewModel.PlaylistName.ToLower());
            bool playlistNameIsAlreadyUsed = existingSmartPlaylists.Any(x => x.Name.ToLower().Equals(editablePlaylistViewModel.PlaylistName.ToLower()));

            if (!isEditingSamePlaylist & playlistNameIsAlreadyUsed)
            {
                return EditPlaylistResult.Duplicate;
            }

            EditPlaylistResult result = EditPlaylistResult.Success;

            try
            {
                PlaylistViewModel playlistViewModel = await this.UpdateSmartPlaylistAsync(editablePlaylistViewModel);

                if (playlistViewModel == null)
                {
                    LogClient.Error($"Error while editing smart playlist '{editablePlaylistViewModel.Path}'. {nameof(playlistViewModel)} is null");
                    result = EditPlaylistResult.Error;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error($"Error while editing smart playlist '{editablePlaylistViewModel.Path}'. Exception: {ex.Message}");
                result = EditPlaylistResult.Error;
            }

            return result;
        }

        public async Task<EditPlaylistResult> EditPlaylistAsync(EditablePlaylistViewModel editablePlaylistViewModel)
        {
            if (editablePlaylistViewModel == null)
            {
                LogClient.Error($"{nameof(editablePlaylistViewModel)} is null");
                return EditPlaylistResult.Error;
            }

            if (string.IsNullOrWhiteSpace(editablePlaylistViewModel.Path))
            {
                LogClient.Error($"{nameof(editablePlaylistViewModel.Path)} is null or empty");
                return EditPlaylistResult.Error;
            }

            if (string.IsNullOrWhiteSpace(editablePlaylistViewModel.PlaylistName))
            {
                LogClient.Error($"{nameof(editablePlaylistViewModel.PlaylistName)} is empty");
                return EditPlaylistResult.Blank;
            }

            this.watcher.Suspend(); // Stop watching the playlist folder

            EditPlaylistResult result = EditPlaylistResult.Error;

            if (editablePlaylistViewModel.Type.Equals(PlaylistType.Static))
            {
                result = await this.EditStaticPlaylistAsync(editablePlaylistViewModel);
            }
            else if (editablePlaylistViewModel.Type.Equals(PlaylistType.Smart))
            {
                result = await this.EditSmartPlaylistAsync(editablePlaylistViewModel);
            }

            this.watcher.Resume(); // Start watching the playlist folder

            if (result == EditPlaylistResult.Success)
            {
                this.PlaylistFolderChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<DeletePlaylistsResult> DeletePlaylistAsync(PlaylistViewModel playlist)
        {
            if (playlist == null)
            {
                LogClient.Error($"{nameof(playlist)} is null");
                return DeletePlaylistsResult.Error;
            }

            DeletePlaylistsResult result = DeletePlaylistsResult.Success;

            this.watcher.Suspend(); // Stop watching the playlist folder

            string filename = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(playlist.Path))
                    {
                        System.IO.File.Delete(playlist.Path);
                    }
                    else
                    {
                        result = DeletePlaylistsResult.Error;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while deleting playlist '{0}'. Exception: {1}", playlist.Path, ex.Message);
                    result = DeletePlaylistsResult.Error;
                }
            });

            if (result == DeletePlaylistsResult.Success)
            {
                this.PlaylistFolderChanged(this, new EventArgs());
            }

            this.watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public async Task SetStaticPlaylistOrderAsync(PlaylistViewModel playlist, IList<TrackViewModel> tracks)
        {
            if (playlist.Type.Equals(PlaylistType.Static))
            {
                LogClient.Error("Cannot set playlist order. This is not a static playlist.");
                return;
            }

            if (playlist == null)
            {
                LogClient.Error($"Cannot set playlist order. {nameof(playlist)} is null.");
                return;
            }

            if (tracks == null || tracks.Count == 0)
            {
                LogClient.Error("Cannot set playlist order. No tracks were provided.");
                return;
            }

            this.watcher.Suspend(); // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    string filename = this.CreatePlaylistFilePath(playlist.Name, PlaylistType.Static);

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

            this.watcher.Resume(); // Start watching the playlist folder
        }

        private async Task<ImportPlaylistResult> ImportStaticPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error("FileName is empty");
                return ImportPlaylistResult.Error;
            }

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
            string sanitizedPlaylistName = this.SanitizePlaylistFilename(playlistName);
            string filename = this.CreatePlaylistFilePath(sanitizedPlaylistName, PlaylistType.Static);

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

            return result;
        }

        private async Task<ImportPlaylistResult> ImportSmartPlaylistAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                LogClient.Error($"{nameof(fileName)} is empty");
                return ImportPlaylistResult.Error;
            }

            IList<PlaylistViewModel> existingPlaylists = await this.GetSmartPlaylistsAsync();

            string newPlaylistName = string.Empty;
            string newFileNameWithoutExtension = string.Empty;
            string newPlaylistFileName = string.Empty;

            ImportPlaylistResult result = ImportPlaylistResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    IList<string> existingPlaylistNames = existingPlaylists.Select(x => x.Name).ToList();
                    IList<string> existingFileNamesWithoutExtension = existingPlaylists.Select(x => Path.GetFileNameWithoutExtension(x.Path)).ToList();

                    string originalPlaylistName = this.GetSmartPlaylistName(fileName);
                    newPlaylistName = originalPlaylistName.MakeUnique(existingPlaylistNames);

                    string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    newFileNameWithoutExtension = originalFileNameWithoutExtension.MakeUnique(existingFileNamesWithoutExtension);

                    // Generate a new filename for the playlist
                    newPlaylistFileName = this.CreatePlaylistFilePath(newFileNameWithoutExtension, PlaylistType.Smart);

                    // Copy the playlist file to the playlists folder, using the new filename.
                    System.IO.File.Copy(fileName, newPlaylistFileName);

                    // Change the playlist name to the unique name (if changed)
                    this.SetSmartPlaylistNameIfDifferent(newPlaylistFileName, newPlaylistName);
                }
                catch (Exception ex)
                {
                    LogClient.Error($"Error while importing smart playlist. Exception: {ex.Message}");
                    result = ImportPlaylistResult.Error;
                }
            });

            return result;
        }

        public async Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames)
        {
            ImportPlaylistResult finalResult = ImportPlaylistResult.Success;

            this.watcher.Suspend(); // Stop watching the playlist folder

            IList<ImportPlaylistResult> results = new List<ImportPlaylistResult>();

            foreach (string fileName in fileNames)
            {
                if (FileFormats.IsSupportedStaticPlaylistFile(fileName))
                {
                    results.Add(await this.ImportStaticPlaylistAsync(fileName));
                }
                else if (FileFormats.IsSupportedSmartPlaylistFile(fileName))
                {
                    results.Add(await this.ImportSmartPlaylistAsync(fileName));
                }
            }

            this.watcher.Resume(); // Start watching the playlist folder


            if (results.Any(x => x.Equals(ImportPlaylistResult.Success)))
            {
                this.PlaylistFolderChanged(this, new EventArgs());
            }

            return finalResult;
        }

        public async Task<EditablePlaylistViewModel> GetEditablePlaylistAsync(PlaylistViewModel playlistViewModel)
        {
            // Assume new playlist
            var editablePlaylist = new EditablePlaylistViewModel(
                await this.GetUniquePlaylistNameAsync(ResourceUtils.GetString("Language_New_Playlist")),
                PlaylistType.Static
                );

            // Not a new playlist
            if (playlistViewModel != null)
            {
                editablePlaylist.Path = playlistViewModel.Path;
                editablePlaylist.PlaylistName = playlistViewModel.Name;
                editablePlaylist.Type = playlistViewModel.Type;

                if (playlistViewModel.Type.Equals(PlaylistType.Smart))
                {
                    await Task.Run(() =>
                    {
                        var decoder = new SmartPlaylistDecoder();
                        DecodeSmartPlaylistResult result = decoder.DecodePlaylist(playlistViewModel.Path);

                        editablePlaylist.Limit = new SmartPlaylistLimitViewModel(result.Limit.Type, result.Limit.Value);
                        editablePlaylist.Limit.IsEnabled = result.Limit.Value > 0;
                        editablePlaylist.SelectedLimitType = editablePlaylist.LimitTypes.Where(x => x.Type.Equals(result.Limit.Type)).FirstOrDefault();
                        editablePlaylist.MatchAnyRule = result.Match.Equals(SmartPlaylistDecoder.MatchAny) ? true : false;

                        var ruleViewModels = new ObservableCollection<SmartPlaylistRuleViewModel>();

                        foreach (SmartPlaylistRule rule in result.Rules)
                        {
                            var ruleViewModel = new SmartPlaylistRuleViewModel();
                            ruleViewModel.SelectedField = ruleViewModel.Fields.Where(x => x.Name.Equals(rule.Field)).FirstOrDefault();

                            // If a invalid field was provided in the xml file, just take the first field.
                            if (ruleViewModel.SelectedField == null)
                            {
                                ruleViewModel.SelectedField = ruleViewModel.Fields.First();
                            }

                            ruleViewModel.SelectedOperator = ruleViewModel.Operators.Where(x => x.Name.Equals(rule.Operator)).FirstOrDefault();

                            // If a invalid operator was provided in the xml file, just take the first operator.
                            if (ruleViewModel.SelectedOperator == null)
                            {
                                ruleViewModel.SelectedOperator = ruleViewModel.Operators.First();
                            }

                            ruleViewModel.Value = rule.Value;

                            ruleViewModels.Add(ruleViewModel);
                        }

                        editablePlaylist.Rules = ruleViewModels;
                    });
                }
            }

            return editablePlaylist;
        }
    }
}
