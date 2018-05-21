using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IGenreRepository
    {
        Task<List<Genre>> GetGenresAsync();
        Task<Genre> GetGenreAsync(string genreName);
        Task<Genre> AddGenreAsync(Genre genre);
        Task DeleteOrphanedGenresAsync();
    }
}
