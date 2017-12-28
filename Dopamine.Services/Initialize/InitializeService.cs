using Dopamine.Services.Contracts.Initialize;
using System.Threading.Tasks;

namespace Dopamine.Services.Initialize
{
    public class InitializeService : IInitializeService
    {
        public bool IsMigrationNeeded()
        {
            throw new System.NotImplementedException();
        }

        public Task MigrateAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}