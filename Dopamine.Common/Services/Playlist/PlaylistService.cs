using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playlist
{
    public class PlaylistService : IPlaylistService
    {
        #region Variables
        private ITrackRepository trackRepository;
        private string playlistFolder;
        #endregion

        #region Construction
        public PlaylistService(ITrackRepository trackRepository)
        {
            // Repositories
            this.trackRepository = trackRepository;

            // Initialize Playlists folder
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string playlistFolder = Path.Combine(musicFolder, ProductInformation.ApplicationDisplayName, "Playlists");

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
        }
        #endregion

        #region Events
        public event EventHandler PlaylistsChanged = delegate { };
        public event Action<int, string> AddedTracksToPlaylist = delegate { };
        public event EventHandler DeletedTracksFromPlaylists = delegate { };
        #endregion

        #region IPlaylistService
        public async Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlist)
        {
            AddToPlaylistResult result = new AddToPlaylistResult();
            //List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(artists), TrackOrder.ByAlbum);
            //AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlist);

            //if (result.IsSuccess)
            //{
            //    this.AddedTracksToPlaylist(result.NumberTracksAdded, playlist);
            //}

            return result;
        }

        public async Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlist)
        {
            AddToPlaylistResult result = new AddToPlaylistResult();

            //List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(genres), TrackOrder.ByAlbum);
            //AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlist);

            //if (result.IsSuccess)
            //{
            //    this.AddedTracksToPlaylist(result.NumberTracksAdded, playlist);
            //}

            return result;
        }

        public async Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlist)
        {
            AddToPlaylistResult result = new AddToPlaylistResult();

            //AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlist);

            //if (result.IsSuccess)
            //{
            //    this.AddedTracksToPlaylist(result.NumberTracksAdded, playlist);
            //}

            return result;
        }

        public async Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlist)
        {
            AddToPlaylistResult result = new AddToPlaylistResult();

            //List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(albums), TrackOrder.ByAlbum);
            //AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlist);

            //if (result.IsSuccess)
            //{
            //    this.AddedTracksToPlaylist(result.NumberTracksAdded, playlist);
            //}

            return result;
        }

        public async Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<PlayableTrack> tracks, string playlist)
        {
            DeleteTracksFromPlaylistsResult result = DeleteTracksFromPlaylistsResult.Success; 

            //DeleteTracksFromPlaylistsResult result = await this.playlistRepository.DeleteTracksFromPlaylistAsync(tracks, selectedPlaylist);

            //if (result == DeleteTracksFromPlaylistsResult.Success)
            //{
            //    this.DeletedTracksFromPlaylists(this, new EventArgs());
            //}

            return result;
        }

        public async Task<RenamePlaylistResult> RenamePlaylistAsync(string oldplaylist, string newplaylist)
        {
            RenamePlaylistResult result = RenamePlaylistResult.Success;

            //RenamePlaylistResult result = await this.playlistRepository.RenamePlaylistAsync(oldplaylist, newplaylist);

            //if (result == RenamePlaylistResult.Success)
            //{
            //    this.PlaylistsChanged(this, new EventArgs());
            //}

            return result;
        }

        public async Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<string> playlists)
        {
            DeletePlaylistResult result = DeletePlaylistResult.Success;

            //DeletePlaylistResult result = await this.playlistRepository.DeletePlaylistsAsync(playlists);

            //if (result == DeletePlaylistResult.Success)
            //{
            //    this.PlaylistsChanged(this, new EventArgs());
            //}

            return result;
        }

        public async Task<AddPlaylistResult> AddPlaylistAsync(string playlist)
        {
            AddPlaylistResult result = AddPlaylistResult.Success;

            //AddPlaylistResult result = await this.playlistRepository.AddPlaylistAsync(playlist);

            //if (result == AddPlaylistResult.Success)
            //{
            //    this.PlaylistsChanged(this, new EventArgs());
            //}

            return result;
        }

        public async Task<List<string>> GetPlaylistsAsync()
        {
            //return await this.playlistRepository.GetPlaylistsAsync();
            return new List<string>();
        }

        public async Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName)
        {
            //string playlist = String.Empty;
            //var paths = new List<String>();

            //// Decode the playlist file
            //// ------------------------
            //var decoder = new PlaylistDecoder();
            //DecodePlaylistResult decodeResult = null;

            //await Task.Run(() => decodeResult = decoder.DecodePlaylist(fileName));

            //if (!decodeResult.DecodeResult.Result)
            //{
            //    LogClient.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
            //    return OpenPlaylistResult.Error;
            //}

            //// Set the paths
            //// -------------
            //paths = decodeResult.Paths;


            //// Get a unique name for the playlist
            //// ----------------------------------
            //playlist = await this.playlistRepository.GetUniqueplaylistAsync(decodeResult.playlist);

            //// Add the Playlist to the database
            //// --------------------------------
            //AddPlaylistResult addPlaylistResult = await this.playlistRepository.AddPlaylistAsync(playlist);
            //if (addPlaylistResult != AddPlaylistResult.Success) return OpenPlaylistResult.Error;

            //// Add Tracks to the Playlist
            //// --------------------------
            //List<PlayableTrack> tracks = await this.trackRepository.GetTracksAsync(paths);
            //AddToPlaylistResult result = await this.playlistRepository.AddTracksToPlaylistAsync(tracks, playlist);
            //if (!result.IsSuccess) return OpenPlaylistResult.Error;

            //// If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success,
            //// so we can always raise the PlaylistsChanged Event.
            //this.PlaylistsChanged(this, new EventArgs());

            return OpenPlaylistResult.Success;
        }

        public async Task<ExportPlaylistsResult> ExportPlaylistAsync(string playlist, string destinationDirectory, bool generateUniqueName)
        {
            //return await this.ExportPlaylistAsync(playlist,
            //                             System.IO.Path.GetDirectoryName(fullPlaylistPath),
            //                             System.IO.Path.GetFileNameWithoutExtension(fullPlaylistPath), generateUniqueName);

            return ExportPlaylistsResult.Success;
        }

        //public async Task<ExportPlaylistsResult> ExportPlaylistAsync(Database.Entities.Playlist playlist, string destinationDirectory, string playlist, bool generateUniqueName)
        //{
        //    ExportPlaylistsResult result = ExportPlaylistsResult.Success;

            //List<PlayableTrack> tracks = await Database.Utils.OrderTracksAsync(await this.trackRepository.GetTracksAsync(playlist.ToList()), TrackOrder.ByFileName);

            //await Task.Run(() =>
            //{

            //    try
            //    {
            //        string playlistFileNameWithoutPathAndExtension = FileUtils.SanitizeFilename(playlist);
            //        string playlistFileFullPath = Path.Combine(destinationDirectory, string.Concat(playlistFileNameWithoutPathAndExtension, FileFormats.M3U));

            //        if (generateUniqueName)
            //        {
            //            // Make sure the file we try to create doesn't exist yet
            //            while (System.IO.File.Exists(playlistFileFullPath))
            //            {
            //                playlistFileNameWithoutPathAndExtension = playlistFileNameWithoutPathAndExtension + " (1)";
            //                playlistFileFullPath = Path.Combine(destinationDirectory, string.Concat(playlistFileNameWithoutPathAndExtension, FileFormats.M3U));
            //            }
            //        }

            //        // Write all the paths to the file
            //        using (StreamWriter file = new StreamWriter(playlistFileFullPath))
            //        {
            //            foreach (PlayableTrack t in tracks)
            //            {
            //                string audioFileNameWithoutPath = Path.GetFileName(t.Path);

            //                // If the audio file is in the same directory as the playlist file, 
            //                // don't save the full path in the playlist file.
            //                if (System.IO.File.Exists(Path.Combine(destinationDirectory, audioFileNameWithoutPath)))
            //                {
            //                    file.WriteLine(audioFileNameWithoutPath);
            //                }
            //                else
            //                {
            //                    file.WriteLine(t.Path);
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        result = ExportPlaylistsResult.Error;
            //        LogClient.Error("Error while exporting Playlist '{0}'. Exception: {1}", playlist.playlist, ex.Message);
            //    }
            //});

        //    return result;
        //}

        public async Task<ExportPlaylistsResult> ExportPlaylistsAsync(IList<string> playlists, string destinationDirectory)
        {
            ExportPlaylistsResult result = ExportPlaylistsResult.Success;

            //foreach (Database.Entities.Playlist pl in playlists)
            //{
            //    ExportPlaylistsResult tempResult = await this.ExportPlaylistAsync(pl, destinationDirectory, pl.playlist, true);

            //    // If at least 1 export failed, return an error
            //    if (tempResult == ExportPlaylistsResult.Error)
            //    {
            //        result = tempResult;
            //    }
            //}

            return result;
        }
        #endregion
    }
}
