using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IArtistRepository
    {
        Task<List<Artist>> GetArtistsAsync(ArtistOrder artistOrder);
        Task<Artist> GetArtistAsync(string artistName);
        Task<Artist> AddArtistAsync(Artist artist);
        Task DeleteOrphanedArtistsAsync();
    }
}
