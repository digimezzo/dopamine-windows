using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Playlist
{
    public delegate void PlaylistAddedHandler(string addedPlaylist);
    public delegate void TracksAddedHandler(int numberTracksAdded, string playlist);
    public delegate void TracksDeletedHandler(List<string> deletedPaths, string playlist);
    public delegate void PlaylistDeletedHandler(string deletedPlaylist);
    public delegate void PlaylistRenamedHandler(string oldPLaylist, string newPlaylist);

    public interface IPlaylistService
    {
        string PlaylistFolder { get; }

        Task<AddPlaylistResult> AddPlaylistAsync(string playlist);
        Task<DeletePlaylistsResult> DeletePlaylistAsync(string playlist);
        Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylist, string newPlaylist);
        Task<List<string>> GetPlaylistsAsync();
        Task<OpenPlaylistResult> OpenPlaylistAsync(string fileName);
        Task<List<PlayableTrack>> GetTracks(string playlist);
        Task SetPlaylistOrderAsync(IList<PlayableTrack> tracks, string playlist);
        Task<AddTracksToPlaylistResult> AddTracksToPlaylistAsync(IList<PlayableTrack> tracks, string playlist);
        Task<AddTracksToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlist);
        Task<AddTracksToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlist);
        Task<AddTracksToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlist);
        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromPlaylistAsync(IList<PlayableTrack> tracks, string playlist);

        event PlaylistAddedHandler PlaylistAdded;
        event PlaylistDeletedHandler PlaylistDeleted;
        event PlaylistRenamedHandler PlaylistRenamed;
        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
    }
}
