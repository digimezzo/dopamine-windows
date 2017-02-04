namespace Dopamine.Common.Services.Playlist
{
    public enum AddPlaylistResult
    {
        Error = 0,
        Success = 1,
        Duplicate = 2,
        Blank = 3
    }

    public enum DeletePlaylistResult
    {
        Error = 0,
        Success = 1
    }

    public enum RenamePlaylistResult
    {
        Error = 0,
        Success = 1,
        Duplicate = 2,
        Blank = 3
    }

    public enum DeleteTracksFromPlaylistsResult
    {
        Error = 0,
        Success = 1
    }

    public enum OpenPlaylistResult
    {
        Error = 0,
        Success = 1
    }

    public enum ExportPlaylistsResult
    {
        Error = 0,
        Success = 1
    }
}
