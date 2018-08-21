using System.Threading.Tasks;

namespace Dopamine.Services.Lifetime
{
    public interface ILifetimeService
    {
        bool MustPerformClosingTasks { get; }

        Task PerformClosingTasksAsync();
    }
}