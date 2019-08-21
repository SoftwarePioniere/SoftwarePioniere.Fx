using System.Threading;

namespace SoftwarePioniere.Hosting
{
    public class SopiApplicationLifetime : ISopiApplicationLifetime
    {
        private readonly CancellationTokenSource _commandHandlerCts = new CancellationTokenSource();

        public CancellationToken CommandHandlerStopped => _commandHandlerCts.Token;

        public void StopCommandHandler()
        {
            if (!_commandHandlerCts.IsCancellationRequested)
                _commandHandlerCts.Cancel();
        }
    }
}