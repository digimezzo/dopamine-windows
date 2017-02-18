using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playlist
{
    public delegate void PlaylistAddedHandler(string addedPlaylistName);
    public delegate void TracksAddedHandler(int numberTracksAdded, string playlistName);
    public delegate void TracksDeletedHandler(List<string> deletedPaths, string playlistName);
    public delegate void PlaylistDeletedHandler(string deletedPlaylistName);
    public delegate void PlaylistRenamedHandler(string oldPLaylistName, string newPlaylistName);

    public interface IPlaylistService
    {
        string PlaylistFolder { get; }

        Task<AddPlaylistResult> AddPlaylistAsync(string playlistName);
        Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlistName);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName);
        Task<List<string>> GetPlaylistsAsync();
        Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName);
        Task<List<PlayableTrack>> GetTracks(string playlistName);
        Task SetPlaylistOrderAsync(IList<PlayableTrack> tracks, string playlistName);
        Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlistName);
        Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName);
        Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName);
        Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName);
        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromPlaylistAsync(IList<PlayableTrack> tracks, string playlistName);

        event PlaylistAddedHandler PlaylistAdded;
        event PlaylistDeletedHandler PlaylistDeleted;
        event PlaylistRenamedHandler PlaylistRenamed;
        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
    }
}
