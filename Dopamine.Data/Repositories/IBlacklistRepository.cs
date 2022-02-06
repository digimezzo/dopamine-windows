using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IBlacklistRepository
    {
        Task AddToBlacklistAsync(IList<Blacklist> tracks);

        Task<bool> RemoveFromBlacklist(long blacklistId);

        Task RemoveAllFromBlacklist();

        Task<bool> IsInBlacklist(string safePath);
    }
}
