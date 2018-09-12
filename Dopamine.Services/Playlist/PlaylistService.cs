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

        private string CreatePlaylistFilePath(string sanitizedPlaylistName, PlaylistType type)
        {
            string extension = type.Equals(PlaylistType.Smart) ? FileFormats.DSPL : FileFormats.M3U;
            return Path.Combine(this.PlaylistFolder, sanitizedPlaylistName + extension);
        }

        private async Task<PlaylistViewModel> CreateNewStaticPlaylistAsync(string playlistName, string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Close() prevents file in use issues
                    System.IO.File.Create(filePath).Close();
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not create playlist '{0}' with filename '{1}'. Exception: {2}", playlistName, filePath, ex.Message);
                return null;
            }

            return new PlaylistViewModel(playlistName, filePath, PlaylistType.Static);
        }

        private async Task<PlaylistViewModel> CreateNewSmartPlaylistAsync(string playlistName, string filePath)
        {
            // TODO
            return null;
        }

        public async Task<CreateNewPlaylistResult> CreateNewPlaylistAsync(string playlistName, PlaylistType type)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                return CreateNewPlaylistResult.Blank;
            }

            string sanitizedPlaylistName = FileUtils.SanitizeFilename(playlistName);
            string filePath = this.CreatePlaylistFilePath(sanitizedPlaylistName, type);

            if (System.IO.File.Exists(filePath))
            {
                return CreateNewPlaylistResult.Duplicate;
            }

            this.watcher.Suspend(); // Stop watching the playlist folder

            PlaylistViewModel playlistViewModel = null;

            if (type.Equals(PlaylistType.Static))
            {
                playlistViewModel = await this.CreateNewStaticPlaylistAsync(playlistName, filePath);
            }
            else if (type.Equals(PlaylistType.Smart))
            {
                playlistViewModel = await this.CreateNewSmartPlaylistAsync(playlistName, filePath);
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

        private string GetWhereOperator(string playlistMatch)
        {
            string whereOperator = string.Empty;

            switch (playlistMatch)
            {
                case "any":
                    whereOperator = "OR";
                    break;
                case "all":
                default:
                    whereOperator = "AND";
                    break;
            }

            return whereOperator;
        }

        private string GetLimit(string playlistLimit)
        {
            string limit = string.Empty;

            long parsedLong = 0;

            if (long.TryParse(playlistLimit, out parsedLong) && parsedLong > 0)
            {
                limit = $"LIMIT {parsedLong}";
            }

            return limit;
        }

        private string GetOrder(string playlistOrder)
        {
            string sqlOrder = string.Empty;

            switch (playlistOrder)
            {
                case "descending":
                    sqlOrder = "DESC";
                    break;
                case "ascending":
                default:
                    sqlOrder = "ASC";
                    break;
            }

            return sqlOrder;
        }

        private string GetWhereClausePart(Rule rule)
        {
            string whereSubClause = string.Empty;

            // Artist
            if (rule.Field.Equals("artist", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Artists LIKE '%{MetadataUtils.DelimitTag(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"Artists LIKE '%{rule.Value}%'";
                }
            }

            // AlbumArtist
            if (rule.Field.Equals("albumartist", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{MetadataUtils.DelimitTag(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{rule.Value}%'";
                }
            }

            // Genre
            if (rule.Field.Equals("genre", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Genres LIKE '%{MetadataUtils.DelimitTag(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"Genres LIKE '%{rule.Value}%'";
                }
            }

            // Title
            if (rule.Field.Equals("title", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"TrackTitle LIKE '%{rule.Value}%'";
                }
            }

            // Title
            if (rule.Field.Equals("albumtitle", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"AlbumTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"AlbumTitle LIKE '%{rule.Value}%'";
                }
            }

            // BitRate
            if (rule.Field.Equals("bitrate", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"BitRate = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"BitRate > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"BitRate < {rule.Value}";
                }
            }

            // TrackNumber
            if (rule.Field.Equals("tracknumber", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"TrackNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"TrackNumber < {rule.Value}";
                }
            }

            // TrackCount
            if (rule.Field.Equals("trackcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"TrackCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"TrackCount < {rule.Value}";
                }
            }

            // DiscNumber
            if (rule.Field.Equals("discnumber", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"DiscNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"DiscNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"DiscNumber < {rule.Value}";
                }
            }

            // DiscCount
            if (rule.Field.Equals("disccount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"DiscCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"DiscCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"DiscCount < {rule.Value}";
                }
            }

            // Year
            if (rule.Field.Equals("year", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Year = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Year > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Year < {rule.Value}";
                }
            }

            // Rating
            if (rule.Field.Equals("rating", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Rating = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Rating > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Rating < {rule.Value}";
                }
            }

            // Love
            if (rule.Field.Equals("love", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Love = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Love > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Love < {rule.Value}";
                }
            }

            // PlayCount
            if (rule.Field.Equals("playcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"PlayCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"PlayCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"PlayCount < {rule.Value}";
                }
            }

            // SkipCount
            if (rule.Field.Equals("skipcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"SkipCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"SkipCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"SkipCount < {rule.Value}";
                }
            }

            return whereSubClause;
        }

        private async Task<IList<TrackViewModel>> GetSmartPlaylistTracksAsync(PlaylistViewModel playlist)
        {
            string whereClause = string.Empty;

            await Task.Run(() =>
            {
                var decoder = new SmartPlaylistDecoder();
                DecodeSmartPlaylistResult result = decoder.DecodePlaylist(playlist.Path);

                string sqlWhereOperator = this.GetWhereOperator(result.Match);
                string sqlLimit = this.GetLimit(result.Limit);
                string sqlOrder = this.GetOrder(result.Order);

                IList<string> whereClauseParts = new List<string>();

                foreach (Rule rule in result.Rules)
                {
                    string whereClausePart = this.GetWhereClausePart(rule);

                    if (!string.IsNullOrWhiteSpace(whereClausePart))
                    {
                        whereClauseParts.Add(whereClausePart);
                    }
                }

                // TODO: orderby

                whereClause = string.Join($" {sqlWhereOperator} ", whereClauseParts.ToArray());
                whereClause = $"({whereClause}) {sqlLimit}";
            });

            if (string.IsNullOrWhiteSpace(whereClause))
            {
                return new List<TrackViewModel>();
            }

            var tracks = await this.trackRepository.GetTracksAsync(whereClause);
            IList<TrackViewModel> trackViewModels = new List<TrackViewModel>();

            try
            {
                trackViewModels = await this.container.ResolveTrackViewModelsAsync(tracks);
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
            else if (playlist.Type.Equals(PlaylistType.Static))
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

        private async Task<RenamePlaylistResult> RenameStaticPlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName)
        {
            string oldFilename = playlistToRename.Path;

            if (!System.IO.File.Exists(oldFilename))
            {
                LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", playlistToRename.Path);
                return RenamePlaylistResult.Error;
            }

            string sanitizedNewPlaylistName = FileUtils.SanitizeFilename(newPlaylistName);
            string newFilename = this.CreatePlaylistFilePath(sanitizedNewPlaylistName, PlaylistType.Static);

            if (System.IO.File.Exists(newFilename))
            {
                return RenamePlaylistResult.Duplicate;
            }

            RenamePlaylistResult result = RenamePlaylistResult.Success;

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

            return result;
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

        private async Task<RenamePlaylistResult> RenameSmartPlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName)
        {
            IList<PlaylistViewModel> existingSmartPlaylists = await this.GetSmartPlaylistsAsync();

            if (existingSmartPlaylists.Any(x => x.Name.ToLower().Equals(newPlaylistName.ToLower())))
            {
                return RenamePlaylistResult.Duplicate;
            }

            RenamePlaylistResult result = RenamePlaylistResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    this.SetSmartPlaylistNameIfDifferent(playlistToRename.Path, newPlaylistName);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while renaming playlist '{0}' to '{1}'. Exception: {2}", playlistToRename.Name, newPlaylistName, ex.Message);
                    result = RenamePlaylistResult.Error;
                }
            });

            return result;
        }

        public async Task<RenamePlaylistResult> RenamePlaylistAsync(PlaylistViewModel playlistToRename, string newPlaylistName)
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


            this.watcher.Suspend(); // Stop watching the playlist folder

            RenamePlaylistResult result = RenamePlaylistResult.Error;

            if (playlistToRename.Type.Equals(PlaylistType.Static))
            {
                result = await this.RenameStaticPlaylistAsync(playlistToRename, newPlaylistName);
            }
            else if (playlistToRename.Type.Equals(PlaylistType.Smart))
            {
                result = await this.RenameSmartPlaylistAsync(playlistToRename, newPlaylistName);
            }

            this.watcher.Resume(); // Start watching the playlist folder

            if (result == RenamePlaylistResult.Success)
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
            string sanitizedPlaylistName = FileUtils.SanitizeFilename(playlistName);
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
    }
}
