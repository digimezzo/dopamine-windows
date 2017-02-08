using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Dopamine.Common.IO;
using System;
using System.Linq;
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
         this.playlistFolder = Path.Combine(musicFolder, ProductInformation.ApplicationDisplayName, "Playlists");

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
      public event PlaylistAddedHandler PlaylistAdded = delegate { };
      public event PlaylistDeletedHandler PlaylistsDeleted = delegate { };
      public event PlaylistRenamedHandler PlaylistRenamed = delegate { };
      public event Action<int, string> AddedTracksToPlaylist = delegate { };
      public event EventHandler DeletedTracksFromPlaylists = delegate { };
      #endregion

      #region Private
      private string CreatePlaylistFilename(string playlist)
      {
         return Path.Combine(this.playlistFolder, playlist + FileFormats.M3U);
      }

      private async Task<string> GetUniquePlaylistAsync(string playlist)
      {
         string uniquePlaylist = playlist;

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
                  uniquePlaylist = playlist + " (" + number + ")";
               }
            });
         }
         catch (Exception ex)
         {
            LogClient.Error("Could not generate unique playlist name for playlist '{0}'. Exception: {1}", playlist, ex.Message);
         }

         return uniquePlaylist;
      }
      #endregion

      #region IPlaylistService
      public async Task<AddPlaylistResult> AddPlaylistAsync(string playlist)
      {
         if (string.IsNullOrWhiteSpace(playlist)) return AddPlaylistResult.Blank;

         string sanitizedPlaylist = FileUtils.SanitizeFilename(playlist);
         string filename = this.CreatePlaylistFilename(sanitizedPlaylist);
         if (System.IO.File.Exists(filename)) return AddPlaylistResult.Duplicate;

         AddPlaylistResult result = AddPlaylistResult.Success;

         await Task.Run(() =>
         {
            try
            {
               System.IO.File.Create(filename);
            }
            catch (Exception ex)
            {
               LogClient.Error("Could not create playlist '{0}' with filename '{1}'. Exception: {2}", playlist, filename, ex.Message);
               result = AddPlaylistResult.Error;
            }
         });

         if (result == AddPlaylistResult.Success) this.PlaylistAdded(sanitizedPlaylist);

         return result;
      }

      public async Task<DeletePlaylistsResult> DeletePlaylistsAsync(IList<string> playlists)
      {
         DeletePlaylistsResult result = DeletePlaylistsResult.Success;
         List<string> deletedPlaylists = new List<string>();

         await Task.Run(() =>
         {
            foreach (string playlist in playlists)
            {
               try
               {
                  string filename = this.CreatePlaylistFilename(playlist);

                  if (System.IO.File.Exists(filename))
                  {
                     System.IO.File.Delete(filename);
                     deletedPlaylists.Add(playlist);
                  }
                  else
                  {
                     result = DeletePlaylistsResult.Error;
                  }
               }
               catch (Exception ex)
               {
                  LogClient.Error("Error while deleting playlist '{0}'. Exception: {1}", playlist, ex.Message);
                  result = DeletePlaylistsResult.Error;
               }
            }
         });

         if (deletedPlaylists.Count > 0) this.PlaylistsDeleted(deletedPlaylists);

         return result;
      }

      public async Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylist, string newPlaylist)
      {
         string oldFilename = this.CreatePlaylistFilename(oldPlaylist);
         if (!System.IO.File.Exists(oldFilename))
         {
            LogClient.Error("Error while renaming playlist. The playlist '{0}' could not be found", oldPlaylist);
            return RenamePlaylistResult.Error;
         }

         string sanitizedNewPlaylist = FileUtils.SanitizeFilename(newPlaylist);
         string newFilename = this.CreatePlaylistFilename(sanitizedNewPlaylist);
         if (System.IO.File.Exists(newFilename)) return RenamePlaylistResult.Duplicate;

         RenamePlaylistResult result = RenamePlaylistResult.Success;

         await Task.Run(() =>
         {
            try
            {
               System.IO.File.Move(oldFilename, newFilename);
            }
            catch (Exception ex)
            {
               LogClient.Error("Error while renaming playlist '{0}' to '{1}'. Exception: {2}", oldPlaylist, newPlaylist, ex.Message);
               result = RenamePlaylistResult.Error;
            }
         });

         if (result == RenamePlaylistResult.Success) this.PlaylistRenamed(oldPlaylist, sanitizedNewPlaylist);

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
         string playlist = String.Empty;
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
            playlist = await this.GetUniquePlaylistAsync(System.IO.Path.GetFileNameWithoutExtension(fileName));
         }
         catch (Exception ex)
         {
            LogClient.Error("Error while getting unique playlist filename. Exception: {0}", ex.Message);
            return OpenPlaylistResult.Error;
         }

         // Create the Playlist in the playlists folder
         // -------------------------------------------
         string sanitizedPlaylist = FileUtils.SanitizeFilename(playlist);
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
                        LogClient.Error("Could not write path '{0}' to playlist '{1}' with filename '{2}'. Exception: {3}", path, playlist, filename, ex.Message);
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            LogClient.Error("Could not create playlist '{0}' with filename '{1}'. Exception: {2}", playlist, filename, ex.Message);
            return OpenPlaylistResult.Error;
         }

         // If we arrive at this point, OpenPlaylistResult = OpenPlaylistResult.Success, so we can always raise the PlaylistAdded Event.
         this.PlaylistAdded(playlist);

         return OpenPlaylistResult.Success;
      }














      // Old

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
