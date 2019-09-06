using System.Threading;

namespace SoftwarePioniere.Hosting
{
    public class SopiApplicationLifetime : ISopiApplicationLifetime
    {
        private readonly CancellationTokenSource _commandHandlerCts = new CancellationTokenSource();

        public CancellationToken Stopped => _commandHandlerCts.Token;

        public void Stop()
        {
            if (!_commandHandlerCts.IsCancellationRequested)
                _commandHandlerCts.Cancel();
        }

        public bool IsStarted { get; set; }
        public bool IsStarting { get; set; }
    }
}