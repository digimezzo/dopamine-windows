using Dopamine.Data.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Data.Contracts.Repositories
{
    public interface IArtistRepository
    {
        Task<List<Artist>> GetArtistsAsync(ArtistOrder artistOrder);
        Task<Artist> GetArtistAsync(string artistName);
        Task<Artist> AddArtistAsync(Artist artist);
        Task DeleteOrphanedArtistsAsync();
    }
}
