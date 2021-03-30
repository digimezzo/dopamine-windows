using System.Threading;

namespace Dopamine.Services.Lifetime
{
    public class TerminationService : ITerminationService
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken
        {
            get => cancellationTokenSource.Token;
        }

        public bool KeepRunning
        { 
            get => !cancellationTokenSource.IsCancellationRequested;
        }

        public bool Cancel()
        {
            if(!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
