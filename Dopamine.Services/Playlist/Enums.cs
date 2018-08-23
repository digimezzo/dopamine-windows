namespace Dopamine.Services.Playlist
{
   public enum AddPlaylistResult
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

   public enum RenamePlaylistResult
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
}
