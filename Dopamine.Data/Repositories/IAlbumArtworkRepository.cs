using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IAlbumArtworkRepository
    {
        Task<IList<AlbumArtwork>> GetAlbumArtworkAsync();

        Task<AlbumArtwork> GetAlbumArtworkAsync(string albumKey);

        Task<AlbumArtwork> GetAlbumArtworkForPathAsync(string path);

        Task DeleteAlbumArtworkAsync(string albumKey);

        Task<long> DeleteUnusedAlbumArtworkAsync();

        Task<IList<string>> GetArtworkIdsAsync();
    }
}
