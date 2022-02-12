using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public interface IBlacklistTrackRepository
    {
        Task AddToBlacklistAsync(IList<BlacklistTrack> tracks);

        Task RemoveFromBlacklistAsync(long blacklistTrackId);

        Task RemoveAllFromBlacklistAsync();

        Task<bool> IsInBlacklistAsync(string safePath);

        Task<IList<BlacklistTrack>> GetBlacklistTracksAsync();
    }
}
