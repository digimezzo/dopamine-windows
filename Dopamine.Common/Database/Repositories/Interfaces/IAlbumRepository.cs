using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAlbumsAsync();
        Task<List<Album>> GetAlbumsAsync(IList<Artist> artists);
        Task<List<Album>> GetAlbumsAsync(IList<Genre> genres);
        Album GetAlbum(long albumID);
        Task<Album> GetAlbumAsync(string albumTitle, string albumArtist);
        Task<List<Album>> GetFrequentAlbumsAsync(int limit);
        Task<Album> AddAlbumAsync(Album album);
        Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID);
        Task<bool> UpdateAlbumAsync(Album album);
        Task DeleteOrphanedAlbumsAsync();
    }
}
