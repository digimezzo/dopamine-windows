namespace Dopamine.Common.Services.Playlist
{
    public enum AddPlaylistResultStatus
    {
        Error = 0,
        Success = 1,
        Duplicate = 2,
        Blank = 3
    }

    public class AddPlaylistResult
    {
        public AddPlaylistResultStatus Status;
        public string AddedPlaylist;
    }
}
