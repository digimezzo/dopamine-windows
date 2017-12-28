using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.Initialize
{
    public interface IInitializeService
    {
        bool IsMigrationNeeded();
        Task MigrateAsync();
    }
}
