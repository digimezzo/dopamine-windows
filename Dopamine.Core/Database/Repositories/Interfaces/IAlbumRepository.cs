using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAlbumsAsync();
        Task<List<Album>> GetAlbumsAsync(IList<Artist> artists);
        Task<List<Album>> GetAlbumsAsync(IList<Genre> genres);
        Task<Album> GetAlbumAsync(string albumTitle, string albumArtist);
        Task<List<Album>> GetAlbumHistoryAsync(int limit);
        Task<Album> AddAlbumAsync(Album album);
        Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID);
        Task<bool> UpdateAlbumAsync(Album album);
        Task DeleteOrphanedAlbumsAsync();
    }
}
