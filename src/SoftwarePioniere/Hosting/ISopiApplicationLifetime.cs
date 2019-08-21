using System.Threading;

namespace SoftwarePioniere.Hosting
{
    public interface ISopiApplicationLifetime
    {
        CancellationToken CommandHandlerStopped { get; }

        void StopCommandHandler();

    }
}
