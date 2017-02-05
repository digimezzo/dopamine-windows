using System.Collections.Generic;

namespace Dopamine.Common.Services.Playlist
{
    public class DeletePlaylistsResult
    {
        #region Properties
        public bool IsSuccess { get; set; }
        public List<string> DeletedPlaylists { get; set; }
        #endregion
    }
}
