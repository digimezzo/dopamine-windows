namespace Dopamine.Common.Database
{
    public enum AddFolderResult
    {
        Error = 0,
        Success = 1,
        Duplicate = 2
    }

    public enum RemoveFolderResult
    {
        Error = 0,
        Success = 1
    }

    public enum ArtistOrder
    {
        Alphabetical = 1,
        ReverseAlphabetical = 2
    }

    public enum GenreOrder
    {
        Alphabetical = 1,
        ReverseAlphabetical = 2
    }

    public enum TrackOrder
    {
        Alphabetical = 1,
        ByAlbum = 2,
        ByFileName = 3,
        ByRating = 4,
        ReverseAlphabetical = 5,
        None = 6
    }

    public enum AlbumOrder
    {
        Alphabetical = 1,
        ByDateAdded = 2,
        ByAlbumArtist = 3,
        ByYear = 4
    }

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

    public enum RemoveTracksResult
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
