using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IPlaylistEntryRepository
    {
        Task DeleteOrphanedPlaylistEntriesAsync();
    }
}
