namespace Dopamine.Services.Playlist
{
    public enum CreateNewPlaylistResult
    {
        Error = 0,
        Success = 1,
        Duplicate = 2,
        Blank = 3
    }

    public enum AddTracksToPlaylistResult
    {
        Error = 0,
        Success = 1
    }

    public enum DeletePlaylistsResult
    {
        Error = 0,
        Success = 1
    }

    public enum EditPlaylistResult
    {
        Error = 0,
        Success = 1,
        Duplicate = 2,
        Blank = 3
    }

    public enum DeleteTracksFromPlaylistResult
    {
        Error = 0,
        Success = 1
    }

    public enum ImportPlaylistResult
    {
        Error = 0,
        Success = 1
    }

    public enum PlaylistType
    {
        Static = 0,
        Smart = 1
    }
}
