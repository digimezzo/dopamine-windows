using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IAlbumRepository
    {
        Task<List<Album>> GetAlbumsAsync();
        Task<List<Album>> GetArtistAlbumsAsync(IList<Artist> artists);
        Task<List<Album>> GetGenreAlbumsAsync(IList<long> genreIds);
        Album GetAlbum(long albumID);
        Task<Album> GetAlbumAsync(string albumTitle, string albumArtist);
        Task<List<Album>> GetFrequentAlbumsAsync(int limit);
        Task<Album> AddAlbumAsync(Album album);
        Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID);
        Task<bool> UpdateAlbumAsync(Album album);
        Task SetAlbumsNeedsIndexing(int needsIndexing, bool onlyUpdateWhenNoCover);
        Task DeleteOrphanedAlbumsAsync();
    }
}
