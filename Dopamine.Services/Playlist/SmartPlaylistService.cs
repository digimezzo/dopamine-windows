using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Helpers;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Data.Repositories;
using Dopamine.Services.Entities;
using Dopamine.Services.Extensions;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistService : PlaylistServiceBase, ISmartPlaylistService
    {
        private IContainerProvider container;
        private ITrackRepository trackRepository;

        public override string PlaylistFolder { get; }

        public override string DialogFileFilter => $"(*{FileFormats.DSPL})|*{FileFormats.DSPL}";

        public SmartPlaylistService(IContainerProvider container, ITrackRepository trackRepository)
        {
            // Dependency injection
            this.container = container;
            this.trackRepository = trackRepository;

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
            var decoder = new SmartPlaylistdecoder();
            DecodeSmartPlaylistResult result = decoder.DecodePlaylist(smartPlaylistPath);

            return result.DecodeResult.Result ? result.PlaylistName : string.Empty;
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

        public override async Task<IList<PlaylistViewModel>> GetPlaylistsAsync()
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
            string newPlaylistFileName = string.Empty;

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
                    newPlaylistFileName = this.CreatePlaylistFilename(newFileNameWithoutExtension);

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
                this.OnPlaylistAdded(new PlaylistViewModel(newPlaylistName, newPlaylistFileName));
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

            IList<PlaylistViewModel> existingPlaylists = await this.GetPlaylistsAsync();

            if (existingPlaylists.Any(x => x.Name.ToLower().Equals(newPlaylistName.ToLower())))
            {
                return RenamePlaylistResult.Duplicate;
            }

            RenamePlaylistResult result = RenamePlaylistResult.Success;

            this.Watcher.Suspend(); // Stop watching the playlist folder

            await Task.Run(() =>
            {
                try
                {
                    this.SetPlaylistNameIfDifferent(playlistToRename.Path, newPlaylistName);
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
                    new PlaylistViewModel(newPlaylistName, playlistToRename.Path));
            }

            this.Watcher.Resume(); // Start watching the playlist folder

            return result;
        }

        public override Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            throw new NotImplementedException();
        }

        public override async Task<IList<TrackViewModel>> GetTracksAsync(PlaylistViewModel playlist)
        {
            if (playlist == null)
            {
                return new List<TrackViewModel>();
            }

            string whereClause = string.Empty;

            await Task.Run(() =>
            {
                var decoder = new SmartPlaylistdecoder();
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
    }
}
