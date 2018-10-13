using Dopamine.Services.Entities;

namespace Dopamine.Services.Playlist
{
    public delegate void TracksAddedHandler(int numberTracksAdded, string playlistName);
    public delegate void TracksDeletedHandler(PlaylistViewModel playlist);
}
