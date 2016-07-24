using Dopamine.Core.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IGenreRepository
    {
        Task<List<Genre>> GetGenresAsync();
        Task<Genre> GetGenreAsync(string genreName);
        Task<Genre> AddGenreAsync(Genre genre);
        Task DeleteOrphanedGenresAsync();
    }
}
