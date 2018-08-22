using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public interface ISmartPlaylistService
    {
        Task<IList<string>> GetSmartPlaylistsAsync();
    }
}
