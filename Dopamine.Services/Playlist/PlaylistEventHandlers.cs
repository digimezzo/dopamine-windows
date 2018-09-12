using Dopamine.Services.Entities;

namespace Dopamine.Services.Playlist
{
    public delegate void PlaylistAddedHandler(PlaylistViewModel addedPlaylist);
    public delegate void TracksAddedHandler(int numberTracksAdded, string playlistName);
    public delegate void TracksDeletedHandler(PlaylistViewModel playlist);
    public delegate void PlaylistDeletedHandler(PlaylistViewModel deletedPlaylist);
    public delegate void PlaylistRenamedHandler(PlaylistViewModel oldPlaylist, PlaylistViewModel newPlaylist);
}
