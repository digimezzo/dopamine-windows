using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playlist
{
   public delegate void PlaylistAddedHandler(string addedPlaylist);
   public delegate void PlaylistDeletedHandler(List<string> deletedPlaylists);
   public delegate void PlaylistRenamedHandler(string oldPLaylist, string newPlaylist);

   public interface IPlaylistService
   {
      Task<AddPlaylistResult> AddPlaylistAsync(string playlist);
      Task<DeletePlaylistsResult> DeletePlaylistsAsync(IList<string> playlists);
      Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylist, string newPlaylist);
      Task<List<string>> GetPlaylistsAsync();
      Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName);
      Task<List<PlayableTrack>> GetTracks(IList<string> playlists);

      event PlaylistAddedHandler PlaylistAdded;
      event PlaylistDeletedHandler PlaylistsDeleted;
      event PlaylistRenamedHandler PlaylistRenamed;

      // Old
      Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlist);
      Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlist);
      Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlist);
      Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlist);
      Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<PlayableTrack> tracks, string playlist);
      
      event Action<int, string> AddedTracksToPlaylist;
      event EventHandler DeletedTracksFromPlaylists;
   }
}
