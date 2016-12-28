using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IPlaylistEntryRepository
    {
        Task DeleteOrphanedPlaylistEntriesAsync();
    }
}
