using System.Threading;

namespace Dopamine.Services.Lifetime
{
    public interface ITerminationService
    {
        bool Cancel();

        CancellationToken CancellationToken { get; }

        bool KeepRunning { get; }
    }
}